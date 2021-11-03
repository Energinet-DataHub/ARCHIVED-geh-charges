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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
using GreenEnergyHub.Charges.TestCore.Database;
using Microsoft.Extensions.Configuration;
using Squadron;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public class ChargesFunctionAppFixture : FunctionAppFixture
    {
        public ChargesFunctionAppFixture(IMessageSink messageSink)
        {
            AzuriteManager = new AzuriteManager();
            DatabaseManager = new ChargesDatabaseManager();
            ServiceBusResource = new AzureCloudServiceBusResource<ChargesFunctionAppServiceBusOptions>(messageSink);
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        [NotNull]
        public ServiceBusListenerMock? PostOfficeListenerMock { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? MessageHubDataAvailableListenerMock { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? MessageHubBundleResponseListenerMock { get; private set; }

        [NotNull]
        public MessageHubMock? MessageHubMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private AzureCloudServiceBusResource<ChargesFunctionAppServiceBusOptions> ServiceBusResource { get; }

        [NotNull]
        private ServiceBusManager? ServiceBusManager { get; set; }

        [NotNull]
        private QueueProperties? BundleReplyQueue { get; set; }

        [NotNull]
        private QueueProperties? BundleRequestQueue { get; set; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            // NOTICE:
            // Currently the following settings must be set on the build agent OR be available in local.settings.json of the function app:
            // * APPINSIGHTS_INSTRUMENTATIONKEY
            // * DOMAINEVENT_SENDER_CONNECTION_STRING
            //
            // All other settings are overwritten somewhere within this class.
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.Currency, "DKK");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, "Europe/Copenhagen");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus
            await ServiceBusResource.InitializeAsync();
            ServiceBusManager = new ServiceBusManager(ServiceBusResource.ConnectionString);

            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            var postOfficeTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.PostOfficeTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.PostOfficeTopicName, postOfficeTopicName);
            PostOfficeListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);
            await PostOfficeListenerMock.AddTopicSubscriptionListenerAsync(postOfficeTopicName, ChargesFunctionAppServiceBusOptions.PostOfficeSubscriptionName);

            // We also overwrite all the service bus connection strings, since we created all topic/queues in just one of them
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventListenerConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubSenderConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubListenerConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubManagerConnectionString, ServiceBusResource.ConnectionString);

            var chargeLinkAcceptedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedTopicName, chargeLinkAcceptedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedSubDataAvailableNotifier, ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedSubEventPublisher, ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedEventPublisherSubscriptionName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedTopicName, chargeLinkAcceptedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedSubDataAvailableNotifier, ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedSubEventPublisher, ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedEventPublisherSubscriptionName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkAcceptedReplier, ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedEventReplierSubscriptionName);

            var chargeLinkCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkCreatedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkCreatedTopicName, chargeLinkCreatedTopicName);

            var chargeLinkReceivedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkReceivedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkReceivedTopicName, chargeLinkReceivedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkReceivedSubscriptionName, ChargesFunctionAppServiceBusOptions.ChargeLinkReceivedSubscriptionName);

            var commandAcceptedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandAcceptedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandAcceptedTopicName, commandAcceptedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandAcceptedSubscriptionName, ChargesFunctionAppServiceBusOptions.CommandAcceptedSubscriptionName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName, ChargesFunctionAppServiceBusOptions.CommandAcceptedReceiverSubscriptionName);

            var commandReceivedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandReceivedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandReceivedTopicName, commandReceivedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandReceivedSubscriptionName, ChargesFunctionAppServiceBusOptions.CommandReceivedSubscriptionName);

            var commandRejectedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandRejectedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandRejectedTopicName, commandRejectedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CommandRejectedSubscriptionName, ChargesFunctionAppServiceBusOptions.CommandRejectedSubscriptionName);

            var createLinkRequestQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkRequestQueueKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CreateLinkRequestQueueName, createLinkRequestQueueName);

            var createLinkReplyQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkReplyQueueName);
            Environment.SetEnvironmentVariable(ChargesFunctionAppServiceBusOptions.CreateLinkReplyQueueKey, createLinkReplyQueueName);

            var createLinkMessagesRequestQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkMessagesRequestQueueKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CreateLinkMessagesRequestQueueName, createLinkMessagesRequestQueueName);

            var createLinkMessagesReplyQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkMessagesReplyQueueName);
            Environment.SetEnvironmentVariable(ChargesFunctionAppServiceBusOptions.CreateLinkMessagesReplyQueueKey, createLinkMessagesReplyQueueName);

            var consumptionMeteringPointCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ConsumptionMeteringPointCreatedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ConsumptionMeteringPointCreatedTopicName, consumptionMeteringPointCreatedTopicName);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ConsumptionMeteringPointCreatdSubscriptionName, ChargesFunctionAppServiceBusOptions.ConsumptionMeteringPointCreatedSubscriptionName);

            var chargeCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeCreatedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeLinkCreatedTopicName, chargeCreatedTopicName);

            var chargePricesUpdatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargePricesUpdatedTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargePricesUpdatedTopicName, chargePricesUpdatedTopicName);

            Environment.SetEnvironmentVariable(ChargesFunctionAppServiceBusOptions.ChargeAcceptedDataAvailableNotifierSubscriptionKey, ChargesFunctionAppServiceBusOptions.ChargeAcceptedDataAvailableNotifierSubscriptionName);

            var messageHubDataAvailableQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.MessageHubDataAvailableQueueKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubDataAvailableQueue, messageHubDataAvailableQueueName);
            MessageHubDataAvailableListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);
            await MessageHubDataAvailableListenerMock.AddQueueListenerAsync(messageHubDataAvailableQueueName);

            BundleRequestQueue = await ServiceBusManager.CreateQueueAsync(ChargesFunctionAppServiceBusOptions.MessageHubRequestQueueKey, 1, null, true);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubBundleRequestQueue, BundleRequestQueue.Name);

            BundleReplyQueue = await ServiceBusManager.CreateQueueAsync(ChargesFunctionAppServiceBusOptions.MessageHubReplyQueueKey, 1, null, true);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubBundleReplyQueue, BundleReplyQueue.Name);
            MessageHubBundleResponseListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);
            await MessageHubBundleResponseListenerMock.AddQueueListenerAsync(BundleReplyQueue.Name);

            MessageHubMock = new MessageHubMock(ServiceBusResource.ConnectionString, BundleRequestQueue.Name, BundleReplyQueue.Name);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageConnectionString, ChargesFunctionAppServiceBusOptions.MessageHubStorageConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageContainer, ChargesFunctionAppServiceBusOptions.MessageHubStorageContainerName);

            // => Database
            await DatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the function app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeDbConnectionString, DatabaseManager.ConnectionString);
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();

            // => Service Bus
            await PostOfficeListenerMock.DisposeAsync();
            await MessageHubDataAvailableListenerMock.DisposeAsync();
            await MessageHubBundleResponseListenerMock.DisposeAsync();
            await ServiceBusResource.DisposeAsync();

            await ServiceBusManager.DeleteQueueAsync(BundleRequestQueue.Name);
            await ServiceBusManager.DeleteQueueAsync(BundleReplyQueue.Name);
            await ServiceBusManager.DisposeAsync();

            // => Database
            await DatabaseManager.DeleteDatabaseAsync();
        }

        private async Task<string> GetTopicNameFromKeyAsync(string topicKey)
        {
            var topicClient = ServiceBusResource.GetTopicClient(topicKey);
            var topicName = topicClient.TopicName;
            await topicClient.CloseAsync();

            return topicName;
        }

        private async Task<string> GetQueueNameFromKeyAsync(string queueKey)
        {
            var queueClient = ServiceBusResource.GetQueueClient(queueKey);
            var queueName = queueClient.QueueName;
            await queueClient.CloseAsync();

            return queueName;
        }
    }
}
