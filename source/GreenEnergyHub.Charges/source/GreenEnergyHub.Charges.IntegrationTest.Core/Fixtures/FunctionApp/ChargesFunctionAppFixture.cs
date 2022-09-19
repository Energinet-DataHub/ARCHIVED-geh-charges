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
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
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
            AuthorizationConfiguration = AuthorizationConfigurationData.CreateAuthorizationConfiguration();
            AuthorizedTestActors = CreateAuthorizedTestActors(AuthorizationConfiguration.ClientApps);
            AsSystemOperator = SetTestActor(AuthorizationConfigurationData.SystemOperator);
            AsGridAccessProvider = SetTestActor(AuthorizationConfigurationData.GridAccessProvider8100000000030);
            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
        }

        public ChargesDatabaseManager ChargesDatabaseManager { get; }

        public MessageHubDatabaseManager MessageHubDatabaseManager { get; }

        [NotNull]
        public ServiceBusTestListener? ChargeCreatedListener { get; private set; }

        [NotNull]
        public ServiceBusTestListener? ChargePricesUpdatedListener { get; private set; }

        [NotNull]
        public MessageHubSimulation? MessageHubMock { get; private set; }

        [NotNull]
        public QueueResource? CreateLinkRequestQueue { get; private set; }

        [NotNull]
        public QueueResource? CreateLinkReplyQueue { get; private set; }

        [NotNull]
        public ServiceBusTestListener? CreateLinkReplyQueueListener { get; private set; }

        [NotNull]
        public TopicResource? IntegrationEventTopic { get; private set; }

        [NotNull]
        public TopicResource? ChargesDomainEventTopic { get; private set; }

        public AuthorizedTestActor AsGridAccessProvider { get; }

        public AuthorizedTestActor AsSystemOperator { get; }

        private B2CAuthorizationConfiguration AuthorizationConfiguration { get; }

        private IEnumerable<AuthorizedTestActor> AuthorizedTestActors { get; }

        private string LocalTimeZoneName { get; } = "Europe/Copenhagen";

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
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, LocalTimeZoneName);
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
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.B2CTenantId, AuthorizationConfiguration.TenantId);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.BackendServiceAppId, AuthorizationConfiguration.BackendApp.AppId);

            // Domain events
            ChargesDomainEventTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargesDomainEventsTopicKey)
                    .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargesDomainEventTopicName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeCommandReceivedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeInformationCommandReceivedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeInformationCommandReceivedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedDataAvailableSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedDataAvailableSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedPublishSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedPublishSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksAcceptedConfirmationSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksAcceptedConfirmationSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksCommandReceivedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksReceivedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksCommandReceivedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeCommandAcceptedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeInformationOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeCommandAcceptedSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeCommandAcceptedPublishSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeInformationOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeCommandAcceptedPublishSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeAcceptedDataAvailableSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeInformationOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeAcceptedDataAvailableSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargePriceCommandReceivedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargePriceCommandReceivedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargePriceCommandReceivedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeCommandRejectedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeInformationOperationsRejectedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeCommandRejectedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargePriceRejectedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargePriceOperationsRejectedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargePriceOperationsRejectedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargePriceConfirmedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargePriceOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargePriceOperationsConfirmedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargePriceConfirmedDataAvailableSubscriptionName)
                    .AddSubjectFilter(nameof(ChargePriceOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargePriceConfirmedDataAvailableSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargePriceConfirmedPublishSubscriptionName)
                    .AddSubjectFilter(nameof(ChargePriceOperationsAcceptedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargePriceConfirmedPublishSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.ChargeLinksCommandRejectedSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksRejectedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.ChargeLinksCommandRejectedSubscriptionName)

                .AddSubscription(ChargesServiceBusResourceNames.DefaultChargeLinksDataAvailableSubscriptionName)
                    .AddSubjectFilter(nameof(ChargeLinksDataAvailableNotifiedEvent))
                    .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.DefaultChargeLinksDataAvailableSubscriptionName)
                .CreateAsync();

            // Integration events
            IntegrationEventTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.IntegrationEventTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.IntegrationEventTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.MarketParticipantChangedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName)
                .AddSubscription(ChargesServiceBusResourceNames.MeteringPointCreatedSubscriptionName)
                .SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName)
                .CreateAsync();

            await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeLinksCreatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeLinksCreatedTopicName)
                .CreateAsync();

            CreateLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinksRequestQueueKey)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinksRequestQueueName)
                .CreateAsync();

            CreateLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesServiceBusResourceNames.CreateLinksReplyQueueKey)
                .CreateAsync();

            var chargeCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargeCreatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargeCreatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName)
                .CreateAsync();

            var chargeCreatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargeCreatedListener.AddTopicSubscriptionListenerAsync(chargeCreatedTopic.Name, ChargesServiceBusResourceNames.ChargeCreatedSubscriptionName);
            ChargeCreatedListener = new ServiceBusTestListener(chargeCreatedListener);

            var chargePricesUpdatedTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargePricesUpdatedTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargePricesUpdatedTopicName)
                .AddSubscription(ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName)
                .CreateAsync();

            var createLinkReplyQueueListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await createLinkReplyQueueListener.AddQueueListenerAsync(CreateLinkReplyQueue.Name);
            CreateLinkReplyQueueListener = new ServiceBusTestListener(createLinkReplyQueueListener);

            var chargePricesUpdatedListener = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);
            await chargePricesUpdatedListener.AddTopicSubscriptionListenerAsync(chargePricesUpdatedTopic.Name, ChargesServiceBusResourceNames.ChargePricesUpdatedSubscriptionName);
            ChargePricesUpdatedListener = new ServiceBusTestListener(chargePricesUpdatedListener);

            await AcquireTokenForTestActorsAsync();

            await InitializeMessageHubAsync();

            await SetUpRequestResponseLoggingAsync();

            // => Database
            await ChargesDatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the function app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(
                EnvironmentSettingNames.ChargeDbConnectionString,
                ChargesDatabaseManager.ConnectionString);
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

        private IEnumerable<AuthorizedTestActor> CreateAuthorizedTestActors(
            IReadOnlyDictionary<string, B2CClientAppSettings> b2cClientAppSettings)
        {
            return b2cClientAppSettings
                .Select(kv => new AuthorizedTestActor(kv.Value, LocalTimeZoneName))
                .ToList();
        }

        private async Task AcquireTokenForTestActorsAsync()
        {
            foreach (var testActor in AuthorizedTestActors)
            {
                await testActor.AddAuthenticationAsync(
                    AuthorizationConfiguration.TenantId,
                    AuthorizationConfiguration.BackendApp);
            }
        }

        private AuthorizedTestActor SetTestActor(string testActorName)
        {
            return AuthorizedTestActors.Single(a => a.B2CClientAppSettings.Name == testActorName);
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

            messageHubSimulationConfig.PeekTimeout = TimeSpan.FromSeconds(60.0);
            messageHubSimulationConfig.WaitTimeout = TimeSpan.FromSeconds(60.0);

            MessageHubMock = new MessageHubSimulation(messageHubSimulationConfig);
        }

        private static async Task SetUpRequestResponseLoggingAsync()
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
