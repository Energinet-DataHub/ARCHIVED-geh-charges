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
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.TestCore.Database;
using GreenEnergyHub.FunctionApp.TestCommon;
using GreenEnergyHub.FunctionApp.TestCommon.Azurite;
using GreenEnergyHub.FunctionApp.TestCommon.FunctionAppHost;
using GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock;
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
        public ServiceBusListenerMock? ServiceBusListenerMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private AzureCloudServiceBusResource<ChargesFunctionAppServiceBusOptions> ServiceBusResource { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            // NOTICE:
            // Currently the following settings must be set on the build agent OR be available in local.settings.json:
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
            await ServiceBusResource.InitializeAsync();

            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            var postOfficeTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.PostOfficeTopicKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.PostOfficeTopicName, postOfficeTopicName);
            ServiceBusListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);
            await ServiceBusListenerMock.AddTopicSubscriptionListenerAsync(postOfficeTopicName, ChargesFunctionAppServiceBusOptions.PostOfficeSubscriptionName);

            // We also overwrite all the service bus connection strings, since we created all topic/queues in just one of them
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable("DOMAINEVENT_LISTENER_CONNECTION_STRING", ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_SENDER_CONNECTION_STRING", ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable("INTEGRATIONEVENT_LISTENER_CONNECTION_STRING", ServiceBusResource.ConnectionString);

            var chargeLinkAcceptedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedTopicKey);
            Environment.SetEnvironmentVariable("CHARGE_LINK_ACCEPTED_TOPIC_NAME", chargeLinkAcceptedTopicName);
            Environment.SetEnvironmentVariable("CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER", ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName);
            Environment.SetEnvironmentVariable("CHARGELINKACCEPTED_SUB_EVENTPUBLISHER", ChargesFunctionAppServiceBusOptions.ChargeLinkAcceptedEventPublisherSubscriptionName);

            var chargeLinkCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkCreatedTopicKey);
            Environment.SetEnvironmentVariable("CHARGE_LINK_CREATED_TOPIC_NAME", chargeLinkCreatedTopicName);

            var chargeLinkReceivedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeLinkReceivedTopicKey);
            Environment.SetEnvironmentVariable("CHARGE_LINK_RECEIVED_TOPIC_NAME", chargeLinkReceivedTopicName);
            Environment.SetEnvironmentVariable("CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.ChargeLinkReceivedSubscriptionName);

            var commandAcceptedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandAcceptedTopicKey);
            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_TOPIC_NAME", commandAcceptedTopicName);
            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.CommandAcceptedSubscriptionName);
            Environment.SetEnvironmentVariable("COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.CommandAcceptedReceiverSubscriptionName);

            var commandReceivedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandReceivedTopicKey);
            Environment.SetEnvironmentVariable("COMMAND_RECEIVED_TOPIC_NAME", commandReceivedTopicName);
            Environment.SetEnvironmentVariable("COMMAND_RECEIVED_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.CommandReceivedSubscriptionName);

            var commandRejectedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CommandRejectedTopicKey);
            Environment.SetEnvironmentVariable("COMMAND_REJECTED_TOPIC_NAME", commandRejectedTopicName);
            Environment.SetEnvironmentVariable("COMMAND_REJECTED_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.CommandRejectedSubscriptionName);

            var createLinkRequestQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkRequestQueueKey);
            Environment.SetEnvironmentVariable("CREATE_LINK_REQUEST_QUEUE_NAME", createLinkRequestQueueName);

            var createLinkReplyQueueName = await GetQueueNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.CreateLinkReplyQueueKey);
            Environment.SetEnvironmentVariable("CREATE_LINK_REPLY_QUEUE_NAME", createLinkReplyQueueName);

            var consumptionMeteringPointCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ConsumptionMeteringPointCreatedTopicKey);
            Environment.SetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME", consumptionMeteringPointCreatedTopicName);
            Environment.SetEnvironmentVariable("CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME", ChargesFunctionAppServiceBusOptions.ConsumptionMeteringPointCreatedSubscriptionName);

            var chargeCreatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargeCreatedTopicKey);
            Environment.SetEnvironmentVariable("CHARGE_CREATED_TOPIC_NAME", chargeCreatedTopicName);

            var chargePricesUpdatedTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.ChargePricesUpdatedTopicKey);
            Environment.SetEnvironmentVariable("CHARGE_PRICES_UPDATED_TOPIC_NAME", chargePricesUpdatedTopicName);

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
            await ServiceBusListenerMock.DisposeAsync();
            await ServiceBusResource.DisposeAsync();

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
