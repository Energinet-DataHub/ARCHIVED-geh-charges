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
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.IntegrationTest.Core.Authorization;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp
{
    public class ChargesFunctionAppFixture : FunctionAppFixture
    {
        public ChargesFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            ChargesDatabaseManager = new ChargesDatabaseManager();
            MessageHubDatabaseManager = new MessageHubDatabaseManager(ChargesDatabaseManager.ConnectionString);
            AuthorizationConfiguration = new AuthorizationConfiguration(
                "volt",
                "u002",
                "integrationtest.local.settings.json",
                "AZURE_SECRETS_KEYVAULT_URL");
            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
            LocalTimeZoneName = GetValueFromSettingsFile("local.settings.json", "VALUES:LOCAL_TIMEZONENAME");
        }

        public string LocalTimeZoneName { get; }

        public ChargesDatabaseManager ChargesDatabaseManager { get; }

        public MessageHubDatabaseManager MessageHubDatabaseManager { get; }

        [NotNull]
        public ServiceBusTestListener? ChargeCreatedListener { get; private set; }

        [NotNull]
        public ServiceBusTestListener? ChargePricesUpdatedListener { get; private set; }

        [NotNull]
        public MessageHubSimulation? MessageHubMock { get; private set; }

        [NotNull]
        public TopicResource? ChargeLinksReceivedTopic { get; private set; }

        [NotNull]
        public QueueResource? CreateLinkRequestQueue { get; private set; }

        [NotNull]
        public QueueResource? CreateLinkReplyQueue { get; private set; }

        [NotNull]
        public ServiceBusTestListener? CreateLinkReplyQueueListener { get; private set; }

        [NotNull]
        public TopicResource? MeteringPointCreatedTopic { get; private set; }

        [NotNull]
        public TopicResource? MarketParticipantChangedTopic { get; private set; }

        [NotNull]
        public TopicResource? ChargeLinksAcceptedTopic { get; private set; }

        public AuthorizationConfiguration AuthorizationConfiguration { get; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
                return;

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\GreenEnergyHub.Charges.FunctionHost\\bin\\{buildConfiguration}\\net6.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.Currency, "DKK");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, "Europe/Copenhagen");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            // We overwrite all the service bus connection strings, since we will create all topics/queues in our shared Service Bus namespace
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventManagerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DomainEventListenerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubListenerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubManagerConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.B2CTenantId, AuthorizationConfiguration.B2cTenantId);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.BackendServiceAppId, AuthorizationConfiguration.BackendAppId);

            ChargeLinksAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinksAcceptedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedDataAvailableNotifierSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedSubDataAvailableNotifier)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedEventPublisherSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedEventReplierSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedReplier)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedConfirmationNotifierSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier)
                .CreateAsync();

            var chargeLinkCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinksCreatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinksCreatedTopicName)
                .CreateAsync();

            ChargeLinksReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinksReceivedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinksReceivedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksReceivedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksReceivedSubscriptionName)
                .CreateAsync();

            var commandAcceptedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandAcceptedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandAcceptedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandAcceptedSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandAcceptedReceiverSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeAcceptedSubDataAvailableNotifier)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier)
                .CreateAsync();

            var commandReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandReceivedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandReceivedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandReceivedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandReceivedSubscriptionName)
                .CreateAsync();

            var priceCommandReceivedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.PriceCommandReceivedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.PriceCommandReceivedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.PriceCommandReceivedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.PriceCommandReceivedSubscriptionName)
                .CreateAsync();

            var commandRejectedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.CommandRejectedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.CommandRejectedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.CommandRejectedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.CommandRejectedSubscriptionName)
                .CreateAsync();

            CreateLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinksRequestQueueKey)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinksRequestQueueName)
                .CreateAsync();

            CreateLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinksReplyQueueKey)
                .CreateAsync();

            var chargeLinksCommandRejectedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinksRejectedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinksRejectedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksRejectedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksRejectedSubscriptionName)
                .CreateAsync();

            MeteringPointCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.MeteringPointCreatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.MeteringPointCreatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.MeteringPointCreatedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName)
                .CreateAsync();

            var chargeCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeCreatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeCreatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName)
                .CreateAsync();

            MarketParticipantChangedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.MarketParticipantChangedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.MarketParticipantChangedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.MarketParticipantChangedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName)
                .CreateAsync();

            await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.DefaultChargeLinksDataAvailableNotifiedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames
                    .DefaultChargeLinksDataAvailableNotifiedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.DefaultChargeLinksDataAvailableNotifiedSubscriptionName)
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedSubscription)
                .CreateAsync();

            var chargeCreatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargeCreatedListener.AddTopicSubscriptionListenerAsync(chargeCreatedTopic.Name, ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName);
            ChargeCreatedListener = new ServiceBusTestListener(chargeCreatedListener);

            var chargePricesUpdatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargePricesUpdatedTopicKey).SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargePricesUpdatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName)
                .CreateAsync();

            var createLinkReplyQueueListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await createLinkReplyQueueListener.AddQueueListenerAsync(CreateLinkReplyQueue.Name);
            CreateLinkReplyQueueListener = new ServiceBusTestListener(createLinkReplyQueueListener);

            var chargePricesUpdatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargePricesUpdatedListener.AddTopicSubscriptionListenerAsync(chargePricesUpdatedTopic.Name, ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName);
            ChargePricesUpdatedListener = new ServiceBusTestListener(chargePricesUpdatedListener);

            await InitializeMessageHubAsync();

            await SetUpRequestResponseLoggingAsync();

            // => Database
            await ChargesDatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the function app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.ChargeDbConnectionString,
                ChargesDatabaseManager.ConnectionString);

            // Only market participant registry thing being tested is connectivity
            // - so for now we just cheat and provide another connection string
            var marketParticipantRegistryConnectionString = ChargesDatabaseManager.ConnectionString;
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.MarketParticipantRegistryDbConnectionString,
                marketParticipantRegistryConnectionString);
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
            await MessageHubMock.DisposeAsync();

            // Listeners
            await ChargeCreatedListener.DisposeAsync();
            await ChargePricesUpdatedListener.DisposeAsync();
            await CreateLinkReplyQueueListener.DisposeAsync();

            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync();

            // => Database
            await ChargesDatabaseManager.DeleteDatabaseAsync();
        }

        private async Task InitializeMessageHubAsync()
        {
            var messageHubDataAvailableQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubDataAvailableQueueKey)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubDataAvailableQueue)
                .CreateAsync();

            var messageHubRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubRequestQueueKey, requiresSession: true)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubRequestQueue)
                .CreateAsync();

            var messageHubReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.MessageHubReplyQueueKey, requiresSession: true)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.MessageHubReplyQueue)
                .CreateAsync();

            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.MessageHubStorageConnectionString,
                ChargesServiceBusResourceNames.MessageHubStorageConnectionString);
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.MessageHubStorageContainer,
                ChargesServiceBusResourceNames.MessageHubStorageContainerName);

            var messageHubSimulationConfig = new MessageHubSimulationConfig(
                ServiceBusResourceProvider.ConnectionString,
                messageHubDataAvailableQueue.Name,
                messageHubRequestQueue.Name,
                messageHubReplyQueue.Name,
                ChargesServiceBusResourceNames.MessageHubStorageConnectionString,
                ChargesServiceBusResourceNames.MessageHubStorageContainerName);

            messageHubSimulationConfig.PeekTimeout = TimeSpan.FromSeconds(20.0);
            messageHubSimulationConfig.WaitTimeout = TimeSpan.FromSeconds(20.0);

            MessageHubMock = new MessageHubSimulation(messageHubSimulationConfig);
        }

        private async Task SetUpRequestResponseLoggingAsync()
        {
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.RequestResponseLoggingConnectionString,
                ChargesServiceBusResourceNames.RequestResponseLoggingConnectionString);

            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.RequestResponseLoggingContainerName,
                ChargesServiceBusResourceNames.RequestResponseLoggingContainerName);

            var storage = new BlobContainerClient(
                ChargesServiceBusResourceNames.RequestResponseLoggingConnectionString,
                ChargesServiceBusResourceNames.RequestResponseLoggingContainerName);

            if (!await storage.ExistsAsync())
                await storage.CreateAsync();
        }

        private static string GetValueFromSettingsFile(string settingsFileName, string valueName)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(settingsFileName, optional: true)
                .AddEnvironmentVariables()
                .Build().GetValue<string>(valueName);
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
