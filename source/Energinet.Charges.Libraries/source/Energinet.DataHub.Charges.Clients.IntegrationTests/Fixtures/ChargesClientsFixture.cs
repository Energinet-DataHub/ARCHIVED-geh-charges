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
using Energinet.DataHub.Charges.Libraries.Common;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Microsoft.Extensions.Configuration;
using Squadron;
using Xunit.Abstractions;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeMessagesLink.Tests.Fixtures
{
    public class ChargesClientsFixture : FunctionAppFixture
    {
        public ChargesClientsFixture(IMessageSink messageSink)
        {
            AzuriteManager = new AzuriteManager();
            ServiceBusResource = new AzureCloudServiceBusResource<ChargesClientsServiceBusOptions>(messageSink);
        }

        [NotNull]
        public ServiceBusListenerMock? ServiceBusListenerMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private AzureCloudServiceBusResource<ChargesClientsServiceBusOptions> ServiceBusResource { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            // NOTICE:
            // Currently the following settings must be set on the build agent
            // OR be available in local.settings.json of the Charges Client package:
            // * APPINSIGHTS_INSTRUMENTATIONKEY
            // * DOMAINEVENT_SENDER_CONNECTION_STRING
            //
            // All other settings are overwritten somewhere within this class.
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.Currency, "DKK");
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimezoneName, "Europe/Copenhagen");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus
            await ServiceBusResource.InitializeAsync().ConfigureAwait(false);

            // Overwrite all the service bus connection strings, since we created all topic/queues in just one of them
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventSenderConnectionString, ServiceBusResource.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventListenerConnectionString, ServiceBusResource.ConnectionString);

            // Overwrite service bus related settings, so the Charges Clients package uses the names we have control of in the test
            ServiceBusListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);

            var createLinkRequestQueueName = await GetQueueNameFromKeyAsync(
                ChargesClientsServiceBusOptions.CreateLinkRequestQueueKey).ConfigureAwait(false);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CreateLinkRequestQueueName, createLinkRequestQueueName);
            await ServiceBusListenerMock.AddQueueListenerAsync(createLinkRequestQueueName).ConfigureAwait(false);

            var createLinkReplyQueueName = await GetQueueNameFromKeyAsync(
                ChargesClientsServiceBusOptions.CreateLinkReplyQueueKey).ConfigureAwait(false);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.CreateLinkReplyQueueName, createLinkReplyQueueName);
            await ServiceBusListenerMock.AddQueueListenerAsync(createLinkReplyQueueName).ConfigureAwait(false);
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
            // ServiceBusListenerMock CAN be null, if something goes wrong in OnInitializeFunctionAppDependenciesAsync()
            if (ServiceBusListenerMock != null)
                await ServiceBusListenerMock.DisposeAsync().ConfigureAwait(false);

            await ServiceBusResource.DisposeAsync().ConfigureAwait(false);
        }

        private async Task<string> GetTopicNameFromKeyAsync(string topicKey)
        {
            var topicClient = ServiceBusResource.GetTopicClient(topicKey);
            var topicName = topicClient.TopicName;
            await topicClient.CloseAsync().ConfigureAwait(false);

            return topicName;
        }

        private async Task<string> GetQueueNameFromKeyAsync(string queueKey)
        {
            var queueClient = ServiceBusResource.GetQueueClient(queueKey);
            var queueName = queueClient.QueueName;
            await queueClient.CloseAsync().ConfigureAwait(false);

            return queueName;
        }
    }
}
