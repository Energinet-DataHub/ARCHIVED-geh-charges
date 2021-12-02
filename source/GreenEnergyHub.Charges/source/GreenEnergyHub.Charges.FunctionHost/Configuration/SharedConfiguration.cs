// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.SimpleInjector;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Peek;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Configuration;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Infrastructure.Function;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.Infrastructure.ServiceBusReplySenderProvider;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Json;
using NodaTime;
using SimpleInjector;
using MarketParticipantRole = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipantRole;
using ProtobufInboundMapperFactory = GreenEnergyHub.Charges.Infrastructure.ProtobufInboundMapperFactory;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class SharedConfiguration
    {
        internal static void ConfigureServices(Container container)
        {
            container.Register<IClock>(() => SystemClock.Instance, Lifestyle.Scoped);
            //container.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            container.Register<IJsonSerializer, JsonSerializer>();
            container.Register<CorrelationIdMiddleware>(Lifestyle.Scoped);
            container.Register<FunctionTelemetryScopeMiddleware>(Lifestyle.Scoped);
            container.Register<MessageMetaDataMiddleware>(Lifestyle.Scoped);
            container.Register<FunctionInvocationLoggingMiddleware>(Lifestyle.Scoped);

            ConfigureSharedDatabase(container);
            ConfigureSharedMessaging(container);
            ConfigureIso8601Services(container);
            ConfigureSharedCim(container);

            var serviceBusConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString);
            var dataAvailableQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue);
            var domainReplyQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue);
            var storageServiceConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageConnectionString);
            var azureBlobStorageContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageContainer);

            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            AddCreateDefaultChargeLinksReplier(container, serviceBusClient);

            container.AddMessageHubCommunication(
                serviceBusConnectionString,
                new MessageHubConfig(dataAvailableQueue, domainReplyQueue),
                storageServiceConnectionString,
                new StorageConfig(azureBlobStorageContainerName));
        }

        private static void AddCreateDefaultChargeLinksReplier(
            Container serviceCollection,
            ServiceBusClient serviceBusClient)
        {
            serviceCollection.Register<IServiceBusReplySenderProvider>(() => new ServiceBusReplySenderProvider(serviceBusClient));
            serviceCollection.Register<ICreateDefaultChargeLinksReplier, CreateDefaultChargeLinksReplier>(Lifestyle.Scoped);
        }

        private static void ConfigureSharedDatabase(Container container)
        {
            container.Register<IChargesDatabaseContext, ChargesDatabaseContext>(Lifestyle.Scoped);
            container.Register<IChargeRepository, ChargeRepository>(Lifestyle.Scoped);
            container.Register<IMeteringPointRepository, MeteringPointRepository>(Lifestyle.Scoped);
            container
                .Register<IAvailableDataRepository<AvailableChargeLinksData>, AvailableChargeLinksDataRepository>(Lifestyle.Scoped);
            container
                .Register<IAvailableDataRepository<AvailableChargeData>, AvailableChargeDataRepository>(Lifestyle.Scoped);
            container
                .Register<IAvailableDataRepository<AvailableChargeLinkReceiptData>, AvailableChargeLinkReceiptDataRepository>(Lifestyle.Scoped);
            container
                .Register<IAvailableDataRepository<AvailableChargeReceiptData>, AvailableChargeReceiptDataRepository>(Lifestyle.Scoped);
        }

        private static void ConfigureSharedMessaging(Container container)
        {
            container.Register<MessageDispatcher>(Lifestyle.Scoped);
            container.Register<ProtobufInboundMapperFactory>(Lifestyle.Scoped);

            container.SendProtobufMessage<ChargeLinkCommandReceived>();
            container.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinksReceivedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinkReceivedTopicName));
        }

        private static void ConfigureSharedCim(Container container)
        {
            container.Register<ICimIdProvider, CimIdProvider>();
            container.Register<IHubSenderConfiguration>(() =>
            {
                var senderId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderId);
                var roleIntText = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderRoleIntEnumValue);
                return new HubSenderConfiguration(
                    senderId,
                    (MarketParticipantRole)int.Parse(roleIntText));
            });
        }

        private static void ConfigureIso8601Services(Container container)
        {
            var timeZoneId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.LocalTimeZoneName);
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            container.Register<IIso8601ConversionConfiguration>(() => timeZoneConfiguration, Lifestyle.Singleton);
            container.Register<IIso8601Durations, Iso8601Durations>(Lifestyle.Singleton);
        }
    }
}
