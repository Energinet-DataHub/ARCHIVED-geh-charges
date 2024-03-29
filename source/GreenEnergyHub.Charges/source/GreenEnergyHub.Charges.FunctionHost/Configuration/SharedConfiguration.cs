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
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware.Storage;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Peek;
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeCreated;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.Infrastructure.ReplySender;
using GreenEnergyHub.Charges.Infrastructure.ReplySender.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Persistence;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class SharedConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(_ =>
            {
                var connectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString);
                return new ServiceBusClient(connectionString);
            });
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();
            serviceCollection.AddScoped<CorrelationIdMiddleware>();
            serviceCollection.AddScoped<IHttpResponseBuilder, HttpResponseBuilder>();
            serviceCollection.AddScoped<FunctionTelemetryScopeMiddleware>();
            serviceCollection.AddScoped<MessageMetaDataMiddleware>();
            serviceCollection.AddScoped<FunctionInvocationLoggingMiddleware>();

            serviceCollection.AddJwtTokenSecurity();
            serviceCollection.AddActorContext();
            serviceCollection.AddApplicationInsightsTelemetryWorkerService();
            serviceCollection.AddDomainEventPublishing();

            ConfigureSharedDatabase(serviceCollection);
            ConfigureSharedMessaging(serviceCollection);
            ConfigureIso8601Services(serviceCollection);
            ConfigureSharedCim(serviceCollection);

            AddRequestResponseLogging(serviceCollection);

            AddCreateDefaultChargeLinksReplier(serviceCollection);
            AddPostOfficeCommunication(serviceCollection);
        }

        private static void AddCreateDefaultChargeLinksReplier(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IServiceBusReplySenderProvider>(sp =>
            {
                var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
                return new ServiceBusReplySenderProvider(serviceBusClient);
            });
            serviceCollection.AddScoped<ICreateDefaultChargeLinksReplier, CreateDefaultChargeLinksReplier>();
        }

        private static void ConfigureSharedDatabase(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<ChargesDatabaseContext>(
                options =>
                {
                    var connectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString);
                    options.UseSqlServer(connectionString, o => o.UseNodaTime());
                });

            serviceCollection.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            serviceCollection.AddDbContext<MessageHubDatabaseContext>(
                options =>
                {
                    var connectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString);
                    options.UseSqlServer(connectionString, o => o.UseNodaTime());
                });
            serviceCollection.AddScoped<IMessageHubDatabaseContext, MessageHubDatabaseContext>();

            serviceCollection.AddScoped<IChargeRepository, ChargeRepository>();
            serviceCollection.AddScoped<IChargeMessageRepository, ChargeMessageRepository>();
            serviceCollection.AddScoped<IChargeHistoryRepository, ChargeHistoryRepository>();
            serviceCollection.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
            serviceCollection.AddScoped<
                IAvailableDataRepository<AvailableChargeLinksData>,
                AvailableDataRepository<AvailableChargeLinksData>>();
            serviceCollection.AddScoped<
                IAvailableDataRepository<AvailableChargeData>,
                AvailableDataRepository<AvailableChargeData>>();
            serviceCollection.AddScoped<
                IAvailableDataRepository<AvailableChargeLinksReceiptData>,
                AvailableDataRepository<AvailableChargeLinksReceiptData>>();
            serviceCollection.AddScoped<
                IAvailableDataRepository<AvailableChargeReceiptData>,
                AvailableDataRepository<AvailableChargeReceiptData>>();
            serviceCollection.AddScoped<
                IAvailableDataRepository<AvailableChargePriceData>,
                AvailableDataRepository<AvailableChargePriceData>>();
            serviceCollection.AddScoped<IMarketParticipantRepository, MarketParticipantRepository>();
            serviceCollection.AddScoped<IGridAreaLinkRepository, GridAreaLinkRepository>();
        }

        private static void ConfigureSharedMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
            serviceCollection.AddScoped<JsonMessageDeserializer>();
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.AddScoped<MessageExtractor>();
            serviceCollection.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();
            serviceCollection.AddScoped<IMessageMetaDataContext, MessageMetaDataContext>();
            serviceCollection.AddSingleton<IJsonSerializer, JsonSerializer>();
            serviceCollection.ConfigureProtobufReception();
            serviceCollection.SendProtobuf<ChargeCreated>();
        }

        private static void ConfigureSharedCim(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICimIdProvider, CimIdProvider>();
        }

        private static void ConfigureIso8601Services(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(_ =>
            {
                var timeZoneId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.LocalTimeZoneName);
                var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
                return timeZoneConfiguration;
            });
            serviceCollection.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }

        /// <summary>
        /// Post office provides a NuGet package to handle the configuration, but it's for SimpleInjector
        /// and thus not applicable in this function host. See also
        /// https://github.com/Energinet-DataHub/geh-post-office/blob/main/source/PostOffice.Communicator.SimpleInjector/source/PostOffice.Communicator.SimpleInjector/ServiceCollectionExtensions.cs
        /// </summary>
        private static void AddPostOfficeCommunication(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(_ =>
            {
                var dataAvailableQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue);
                var messageHubReplyQueue = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue);
                var messageHubConfig = new MessageHubConfig(dataAvailableQueue, messageHubReplyQueue);
                return messageHubConfig;
            });
            serviceCollection.AddSingleton(_ =>
            {
                var azureBlobStorageContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageContainer);
                var storageConfig = new StorageConfig(azureBlobStorageContainerName);
                return storageConfig;
            });
            serviceCollection.AddServiceBus();
            serviceCollection.AddApplicationServices();
            serviceCollection.AddStorageHandler();
        }

        private static void AddRequestResponseLogging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IRequestResponseLogging>(provider =>
            {
                var requestResponseLogStorage = EnvironmentHelper.GetEnv(EnvironmentSettingNames.RequestResponseLoggingConnectionString);
                var requestResponseLogContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.RequestResponseLoggingContainerName);

                var storageLogging = provider.GetService<ILogger<RequestResponseLoggingBlobStorage>>();
                return new RequestResponseLoggingBlobStorage(requestResponseLogStorage, requestResponseLogContainerName, storageLogging!);
            });
        }

        private static void AddServiceBus(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IServiceBusClientFactory>(_ =>
            {
                var serviceBusConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString);
                if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid ServiceBus in the appSettings.json file or your Azure Functions Settings.");
                }

                return new ServiceBusClientFactory(serviceBusConnectionString);
            });

            serviceCollection.AddSingleton<IMessageBusFactory>(provider =>
            {
                var serviceBusClientFactory = provider.GetRequiredService<IServiceBusClientFactory>();
                return new AzureServiceBusFactory(serviceBusClientFactory);
            });
        }

        private static void AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDataAvailableNotificationSender, DataAvailableNotificationSender>();
            serviceCollection.AddSingleton<IResponseBundleParser, ResponseBundleParser>();
            serviceCollection.AddSingleton<IDataBundleResponseSender, DataBundleResponseSender>();
            serviceCollection.AddSingleton<IDequeueNotificationParser, DequeueNotificationParser>();
        }

        private static void AddStorageHandler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IStorageServiceClientFactory>(_ =>
            {
                var storageServiceConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubStorageConnectionString);
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
