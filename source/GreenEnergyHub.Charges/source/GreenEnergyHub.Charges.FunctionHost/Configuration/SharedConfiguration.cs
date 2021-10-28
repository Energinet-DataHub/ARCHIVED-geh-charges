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
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Factories;
using Energinet.DataHub.Charges.Libraries.ServiceBus;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Dequeue;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

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
            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                EnvironmentHelper.GetEnv("APPINSIGHTS_INSTRUMENTATIONKEY"));

            ConfigureSharedDatabase(serviceCollection);
            ConfigureSharedMessaging(serviceCollection);

            var serviceBusConnectionString = EnvironmentHelper.GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING");
            var dataAvailableQueue = EnvironmentHelper.GetEnv("MESSAGEHUB_DATAAVAILABLE_QUEUE");
            var domainReplyQueue = EnvironmentHelper.GetEnv("MESSAGEHUB_BUNDLEREPLY_QUEUE");
            var storageServiceConnectionString = EnvironmentHelper.GetEnv("MESSAGEHUB_STORAGE_CONNECTION_STRING");
            var azureBlobStorageContainerName = EnvironmentHelper.GetEnv("MESSAGEHUB_STORAGE_CONTAINER");
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
            var replyToQueueName = EnvironmentHelper.GetEnv("CREATE_LINK_REPLY_QUEUE_NAME");
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
        }

        private static void ConfigureSharedMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.ConfigureProtobufReception();

            serviceCollection.SendProtobuf<ChargeLinkCommandReceived>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
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
