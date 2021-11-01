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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.TestCore.Database;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public class ChargesFunctionAppFixture : FunctionAppFixture
    {
        public ChargesFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            DatabaseManager = new ChargesDatabaseManager();

            var integrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(integrationTestConfiguration.ServiceBusConnectionString);
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        [NotNull]
        public ServiceBusListenerMock? PostOfficeListenerMock { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? DataAvailableListenerMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

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

            Environment.SetEnvironmentVariable("CURRENCY", "DKK");
            Environment.SetEnvironmentVariable("LOCAL_TIMEZONENAME", "Europe/Copenhagen");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus: Overwrite service bus related settings, so the function app uses the names we have control of in the test
            var postOfficeTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.PostOfficeTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.PostOfficeSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.PostOfficeTopicName, postOfficeTopic.Name);

            PostOfficeListenerMock = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await PostOfficeListenerMock.AddTopicSubscriptionListenerAsync(postOfficeTopic.Name, ChargesServiceBusResourceNames.PostOfficeSubscriptionName);

            // We also overwrite all the service bus connection strings, since we created all topic/queues in just one of them
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("DOMAINEVENT_LISTENER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_SENDER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_LISTENER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_MANAGER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            var chargeLinkAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkAcceptedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventPublisherSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventReplierSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CHARGE_LINK_ACCEPTED_TOPIC_NAME", chargeLinkAcceptedTopic.Name);
            Environment.SetEnvironmentVariable("CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER", ChargesServiceBusResourceNames.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName);
            Environment.SetEnvironmentVariable("CHARGELINKACCEPTED_SUB_EVENTPUBLISHER", ChargesServiceBusResourceNames.ChargeLinkAcceptedEventPublisherSubscriptionName);
            Environment.SetEnvironmentVariable("CHARGELINKACCEPTED_SUB_REPLIER", ChargesServiceBusResourceNames.ChargeLinkAcceptedEventReplierSubscriptionName);

            var chargeLinkCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkCreatedTopicKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CHARGE_LINK_CREATED_TOPIC_NAME", chargeLinkCreatedTopic.Name);

            var chargeLinkReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkReceivedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkReceivedSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CHARGE_LINK_RECEIVED_TOPIC_NAME", chargeLinkReceivedTopic.Name);
            Environment.SetEnvironmentVariable("CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.ChargeLinkReceivedSubscriptionName);

            var commandAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandAcceptedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedReceiverSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_TOPIC_NAME", commandAcceptedTopic.Name);
            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.CommandAcceptedSubscriptionName);
            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.CommandAcceptedReceiverSubscriptionName);

            var commandReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandReceivedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.CommandReceivedSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("COMMAND_RECEIVED_TOPIC_NAME", commandReceivedTopic.Name);
            Environment.SetEnvironmentVariable("COMMAND_RECEIVED_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.CommandReceivedSubscriptionName);

            var commandRejectedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandRejectedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.CommandReceivedSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("COMMAND_REJECTED_TOPIC_NAME", commandRejectedTopic.Name);
            Environment.SetEnvironmentVariable("COMMAND_REJECTED_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.CommandRejectedSubscriptionName);

            var createLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkRequestQueueKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CREATE_LINK_REQUEST_QUEUE_NAME", createLinkRequestQueue.Name);

            var createLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkReplyQueueKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CREATE_LINK_REPLY_QUEUE_NAME", createLinkReplyQueue.Name);

            var consumptionMeteringPointCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedTopicKey)
                .AddSubscription(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedSubscriptionName)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME", consumptionMeteringPointCreatedTopic.Name);
            Environment.SetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME", ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedSubscriptionName);

            var chargeCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeCreatedTopicKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CHARGE_CREATED_TOPIC_NAME", chargeCreatedTopic.Name);

            var chargePricesUpdatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargePricesUpdatedTopicKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("CHARGE_PRICES_UPDATED_TOPIC_NAME", chargePricesUpdatedTopic.Name);

            var messageHubDataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubDataAvailableQueueKey)
                .CreateAsync();

            Environment.SetEnvironmentVariable("MESSAGEHUB_DATAAVAILABLE_QUEUE", messageHubDataAvailableQueue.Name);

            DataAvailableListenerMock = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await DataAvailableListenerMock.AddQueueListenerAsync(messageHubDataAvailableQueue.Name);

            var bundleRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubRequestQueueKey, requireSession: true)
                .CreateAsync();

            Environment.SetEnvironmentVariable("MESSAGEHUB_BUNDLEREQUEST_QUEUE", bundleRequestQueue.Name);

            var bundleReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubReplyQueueKey, requireSession: true)
                .CreateAsync();

            Environment.SetEnvironmentVariable("MESSAGEHUB_BUNDLEREPLY_QUEUE", bundleReplyQueue.Name);

            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONNECTION_STRING", ChargesServiceBusResourceNames.MessageHubStorageConnectionString);

            Environment.SetEnvironmentVariable("MESSAGEHUB_STORAGE_CONTAINER", ChargesServiceBusResourceNames.MessageHubStorageContainerName);

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
            await DataAvailableListenerMock.DisposeAsync();
            await ServiceBusResourceProvider.DisposeAsync();

            // => Database
            await DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
