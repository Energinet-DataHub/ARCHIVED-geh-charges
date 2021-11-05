﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ToBeRenamedAndSplitted;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Function;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.Infrastructure.ToBeRenamedAndSplitted;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using MarketParticipantRole = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipantRole;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class SharedConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<CorrelationIdMiddleware>();
            serviceCollection.AddScoped<MessageMetaDataMiddleware>();
            serviceCollection.AddScoped<FunctionInvocationLoggingMiddleware>();
            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.AppInsightsInstrumentationKey));

            ConfigureSharedDatabase(serviceCollection);
            ConfigureSharedMessaging(serviceCollection);
            ConfigureIso8601Services(serviceCollection);
            ConfigureSharedCim(serviceCollection);

            var serviceBusConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString);
            var dataAvailableQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue);
            var domainReplyQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue);
            var storageServiceConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageConnectionString);
            var azureBlobStorageContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageContainer);
            AddPostOfficeCommunication(
                serviceCollection,
                serviceBusConnectionString,
                new MessageHubConfig(dataAvailableQueue, domainReplyQueue),
                storageServiceConnectionString,
                new StorageConfig(azureBlobStorageContainerName));
            AddDefaultChargeLinkClient(serviceCollection, serviceBusConnectionString);
        }

        private static void AddDefaultChargeLinkClient(
            IServiceCollection serviceCollection,
            string serviceBusConnectionString)
        {
            var replyToQueueName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinkReplyQueueName);
            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            serviceCollection.AddScoped<IServiceBusRequestSenderFactory>(_ =>
                new ServiceBusRequestSenderFactory());
            serviceCollection.AddScoped<IServiceBusRequestSender>(_ =>
                new ServiceBusRequestSender(serviceBusClient, replyToQueueName));
            serviceCollection.AddSingleton<IDefaultChargeLinkClient>(_ =>
                new DefaultChargeLinkClient(serviceBusClient, new ServiceBusRequestSenderFactory(), replyToQueueName));
        }

        private static void ConfigureSharedDatabase(IServiceCollection serviceCollection)
        {
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentSettingNames.ChargeDbConnectionString) ??
                                   throw new ArgumentNullException(
                                       EnvironmentSettingNames.ChargeDbConnectionString,
                                       "does not exist in configuration settings");
            serviceCollection.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString, o => o.UseNodaTime()));
            serviceCollection.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();

            serviceCollection.AddScoped<IChargeRepository, ChargeRepository>();
            serviceCollection.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
            serviceCollection
                .AddScoped<IAvailableChargeLinksDataRepository, AvailableChargeLinksDataRepository>();
            serviceCollection
                .AddScoped<IAvailableChargeDataRepository, AvailableChargeDataRepository>();
        }

        private static void ConfigureSharedMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.ConfigureProtobufReception();

            serviceCollection.SendProtobuf<ChargeLinkCommandReceived>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinkReceivedTopicName));
        }

        private static void ConfigureSharedCim(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICimIdProvider, CimIdProvider>();
            serviceCollection.AddScoped<IHubSenderConfiguration>(_ =>
            {
                var senderId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderId);
                var roleIntText = EnvironmentHelper.GetEnv(EnvironmentSettingNames.HubSenderRoleIntEnumValue);
                return new HubSenderConfiguration(
                    senderId,
                    (MarketParticipantRole)int.Parse(roleIntText));
            });
        }

        private static void ConfigureIso8601Services(IServiceCollection serviceCollection)
        {
            var timeZoneId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.LocalTimeZoneName);
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            serviceCollection.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }

        /// <summary>
        /// Post office provides a NuGet package to handle the configuration, but it's for SimpleInjector
        /// and thus not applicable in this function host. See also
        /// https://github.com/Energinet-DataHub/geh-post-office/blob/main/source/PostOffice.Communicator.SimpleInjector/source/PostOffice.Communicator.SimpleInjector/ContainerExtensions.cs
        /// </summary>
        private static void AddPostOfficeCommunication(
            IServiceCollection serviceCollection,
            string serviceBusConnectionString,
            MessageHubConfig messageHubConfig,
            string storageServiceConnectionString,
            StorageConfig storageConfig)
        {
            if (serviceCollection == null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                throw new ArgumentNullException(nameof(serviceBusConnectionString));

            if (messageHubConfig == null)
                throw new ArgumentNullException(nameof(messageHubConfig));

            if (string.IsNullOrWhiteSpace(storageServiceConnectionString))
                throw new ArgumentNullException(nameof(storageServiceConnectionString));

            if (storageConfig == null)
                throw new ArgumentNullException(nameof(storageConfig));

            serviceCollection.AddSingleton(_ => messageHubConfig);
            serviceCollection.AddSingleton(_ => storageConfig);
            serviceCollection.AddServiceBus(serviceBusConnectionString);
            serviceCollection.AddApplicationServices();
            serviceCollection.AddStorageHandler(storageServiceConnectionString);
        }

        private static void AddServiceBus(this IServiceCollection serviceCollection, string serviceBusConnectionString)
        {
            serviceCollection.AddSingleton<IServiceBusClientFactory>(_ =>
            {
                if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(serviceBusConnectionString);
            });
        }

        private static void AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDataAvailableNotificationSender, DataAvailableNotificationSender>();
            serviceCollection.AddSingleton<IRequestBundleParser, RequestBundleParser>();
            serviceCollection.AddSingleton<IResponseBundleParser, ResponseBundleParser>();
            serviceCollection.AddSingleton<IDataBundleResponseSender, DataBundleResponseSender>();
            serviceCollection.AddSingleton<IDequeueNotificationParser, DequeueNotificationParser>();
        }

        private static void AddStorageHandler(this IServiceCollection serviceCollection, string storageServiceConnectionString)
        {
            serviceCollection.AddSingleton<IStorageServiceClientFactory>(_ =>
            {
                if (string.IsNullOrWhiteSpace(storageServiceConnectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid BlobStorageConnectionString in the appSettings.json file or your Azure Functions Settings.");
                }

                return new StorageServiceClientFactory(storageServiceConnectionString);
            });

            serviceCollection.AddSingleton<IStorageHandler, StorageHandler>();
        }
    }
}
