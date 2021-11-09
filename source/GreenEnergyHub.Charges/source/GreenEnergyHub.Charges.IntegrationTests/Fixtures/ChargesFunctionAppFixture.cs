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
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
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
            ServiceBusResourceProvider = new ServiceBusResourceProvider(integrationTestConfiguration.ServiceBusConnectionString, new TestDiagnosticsLogger());
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        [NotNull]
        public ServiceBusTestListener? ChargeCreatedListener { get; private set; }

        [NotNull]
        public ServiceBusTestListener? ChargePricesUpdatedListener { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? PostOfficeListener { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? MessageHubDataAvailableListener { get; private set; }

        [NotNull]
        public ServiceBusListenerMock? MessageHubReplyListener { get; private set; }

        [NotNull]
        public MessageHubMock? MessageHubMock { get; private set; }

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

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.Currency, "DKK");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, "Europe/Copenhagen");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.HubSenderId, "5790001330552");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.HubSenderRoleIntEnumValue, "7");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            // We overwrite all the service bus connection strings, since we will create all topics/queues in our shared Service Bus namespace
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventListenerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubListenerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubManagerConnectionString, ServiceBusResourceProvider.ConnectionString);

            var postOfficeTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.PostOfficeTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.PostOfficeTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.PostOfficeSubscriptionName)
                .CreateAsync();

            PostOfficeListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await PostOfficeListener.AddTopicSubscriptionListenerAsync(postOfficeTopic.Name, ChargesServiceBusResourceNames.PostOfficeSubscriptionName);

            var chargeLinkAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkAcceptedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinkAcceptedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedDataAvailableNotifierSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinkAcceptedSubDataAvailableNotifier)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventPublisherSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinkAcceptedSubEventPublisher)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkAcceptedEventReplierSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinkAcceptedReplier)
                .CreateAsync();

            var chargeLinkCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkCreatedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinkCreatedTopicName)
                .CreateAsync();

            var chargeLinkReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinkReceivedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinkReceivedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinkReceivedSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinkReceivedSubscriptionName)
                .CreateAsync();

            var commandAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandAcceptedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandAcceptedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandAcceptedSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedReceiverSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeAcceptedSubDataAvailableNotifier).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier)
                .CreateAsync();

            var commandReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandReceivedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandReceivedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandReceivedSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandReceivedSubscriptionName)
                .CreateAsync();

            var commandRejectedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandRejectedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandRejectedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandRejectedSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandRejectedSubscriptionName)
                .CreateAsync();

            var createLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkRequestQueueKey).SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinkRequestQueueName)
                .CreateAsync();

            var createLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinkReplyQueueKey).SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinkReplyQueueName)
                .CreateAsync();

            var consumptionMeteringPointCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ConsumptionMeteringPointCreatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ConsumptionMeteringPointCreatedSubscriptionName).SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ConsumptionMeteringPointCreatdSubscriptionName)
                .CreateAsync();

            var chargeCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeCreatedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeCreatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName)
                .CreateAsync();

            var chargeCreatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargeCreatedListener.AddTopicSubscriptionListenerAsync(chargeCreatedTopic.Name, ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName);
            ChargeCreatedListener = new ServiceBusTestListener(chargeCreatedListener);

            var chargePricesUpdatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargePricesUpdatedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargePricesUpdatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName)
                .CreateAsync();

            var chargePricesUpdatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargePricesUpdatedListener.AddTopicSubscriptionListenerAsync(chargePricesUpdatedTopic.Name, ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName);
            ChargePricesUpdatedListener = new ServiceBusTestListener(chargePricesUpdatedListener);

            var messageHubDataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubDataAvailableQueueKey).SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubDataAvailableQueue)
                .CreateAsync();

            MessageHubDataAvailableListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await MessageHubDataAvailableListener.AddQueueListenerAsync(messageHubDataAvailableQueue.Name);

            var messageHubRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubRequestQueueKey, requireSession: true).SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubRequestQueue)
                .CreateAsync();

            var messageHubReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubReplyQueueKey, requireSession: true).SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubReplyQueue)
                .CreateAsync();

            MessageHubReplyListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);

            // BUG: This code doesn't work. See bug https://github.com/Energinet-DataHub/geh-charges/issues/788
            //await MessageHubReplyListener.AddQueueListenerAsync(messageHubReplyQueue.Name);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageConnectionString, ChargesServiceBusResourceNames.MessageHubStorageConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.MessageHubStorageContainer, ChargesServiceBusResourceNames.MessageHubStorageContainerName);

            MessageHubMock = new MessageHubMock(ServiceBusResourceProvider.ConnectionString, messageHubRequestQueue.Name, messageHubReplyQueue.Name);

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
            await PostOfficeListener.DisposeAsync();
            await MessageHubDataAvailableListener.DisposeAsync();
            await MessageHubReplyListener.DisposeAsync();
            await ServiceBusResourceProvider.DisposeAsync();

            // => Database
            await DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
