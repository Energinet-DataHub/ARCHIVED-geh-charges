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

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            // We overwrite all the service bus connection strings, since we will create all topics/queues in our shared Service Bus namespace
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("DOMAINEVENT_LISTENER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_SENDER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_LISTENER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_MANAGER_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            var postOfficeTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.PostOfficeTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.PostOfficeTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.PostOfficeSubscriptionName)
                .CreateAsync();

            PostOfficeListenerMock = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await PostOfficeListenerMock.AddTopicSubscriptionListenerAsync(postOfficeTopic.Name, ChargesServiceBusResourceNames.PostOfficeSubscriptionName);

            var chargeLinkAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkAcceptedTopicKey).SetEnvironmentVariableToTopicName("CHARGE_LINK_ACCEPTED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName).SetEnvironmentVariableToSubscriptionName("CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER")
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventPublisherSubscriptionName).SetEnvironmentVariableToSubscriptionName("CHARGELINKACCEPTED_SUB_EVENTPUBLISHER")
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventReplierSubscriptionName).SetEnvironmentVariableToSubscriptionName("CHARGELINKACCEPTED_SUB_REPLIER")
                .CreateAsync();

            var chargeLinkCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkCreatedTopicKey).SetEnvironmentVariableToTopicName("CHARGE_LINK_CREATED_TOPIC_NAME")
                .CreateAsync();

            var chargeLinkReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkReceivedTopicKey).SetEnvironmentVariableToTopicName("CHARGE_LINK_RECEIVED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkReceivedSubscriptionName).SetEnvironmentVariableToSubscriptionName("CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME")
                .CreateAsync();

            var commandAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandAcceptedTopicKey).SetEnvironmentVariableToTopicName("COMMAND_ACCEPTED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedSubscriptionName).SetEnvironmentVariableToSubscriptionName("COMMAND_ACCEPTED_SUBSCRIPTION_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedReceiverSubscriptionName).SetEnvironmentVariableToSubscriptionName("COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME")
                .CreateAsync();

            var commandReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandReceivedTopicKey).SetEnvironmentVariableToTopicName("COMMAND_RECEIVED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.CommandReceivedSubscriptionName).SetEnvironmentVariableToSubscriptionName("COMMAND_RECEIVED_SUBSCRIPTION_NAME")
                .CreateAsync();

            var commandRejectedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandRejectedTopicKey).SetEnvironmentVariableToTopicName("COMMAND_REJECTED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.CommandRejectedSubscriptionName).SetEnvironmentVariableToSubscriptionName("COMMAND_REJECTED_SUBSCRIPTION_NAME")
                .CreateAsync();

            var createLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkRequestQueueKey).SetEnvironmentVariableToQueueName("CREATE_LINK_REQUEST_QUEUE_NAME")
                .CreateAsync();

            var createLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkReplyQueueKey).SetEnvironmentVariableToQueueName("CREATE_LINK_REPLY_QUEUE_NAME")
                .CreateAsync();

            var consumptionMeteringPointCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedTopicKey).SetEnvironmentVariableToTopicName("CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME")
                .AddSubscription(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedSubscriptionName).SetEnvironmentVariableToSubscriptionName("CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME")
                .CreateAsync();

            var chargeCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeCreatedTopicKey).SetEnvironmentVariableToTopicName("CHARGE_CREATED_TOPIC_NAME")
                .CreateAsync();

            var chargePricesUpdatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargePricesUpdatedTopicKey).SetEnvironmentVariableToTopicName("CHARGE_PRICES_UPDATED_TOPIC_NAME")
                .CreateAsync();

            var messageHubDataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubDataAvailableQueueKey).SetEnvironmentVariableToQueueName("MESSAGEHUB_DATAAVAILABLE_QUEUE")
                .CreateAsync();

            DataAvailableListenerMock = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await DataAvailableListenerMock.AddQueueListenerAsync(messageHubDataAvailableQueue.Name);

            var bundleRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubRequestQueueKey, requireSession: true).SetEnvironmentVariableToQueueName("MESSAGEHUB_BUNDLEREQUEST_QUEUE")
                .CreateAsync();

            var bundleReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubReplyQueueKey, requireSession: true).SetEnvironmentVariableToQueueName("MESSAGEHUB_BUNDLEREPLY_QUEUE")
                .CreateAsync();

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
