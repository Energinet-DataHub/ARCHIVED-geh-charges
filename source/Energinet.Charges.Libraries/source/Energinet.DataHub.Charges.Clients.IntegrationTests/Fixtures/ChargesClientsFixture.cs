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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Charges.Clients.IntegrationTests.Fixtures
{
    public class ChargesClientsFixture : LibraryFixture
    {
        public ChargesClientsFixture()
        {
            AzuriteManager = new AzuriteManager();
            var integrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                integrationTestConfiguration.ServiceBusConnectionString, new TestDiagnosticsLogger());
        }

        [NotNull]
        public ServiceBusListenerMock? ServiceBusListenerMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override async Task OnInitializeLibraryDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // Overwrite all the service bus connection strings, since we created all topic/queues in just one of them
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventSenderConnectionString, ServiceBusResourceProvider.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventListenerConnectionString, ServiceBusResourceProvider.ConnectionString);

            // Overwrite service bus related settings, so the Charges Clients package uses the names we have control of in the test
            ServiceBusListenerMock = new ServiceBusListenerMock(ServiceBusResourceProvider.ConnectionString, TestLogger);

            var createLinkRequestQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesClientsServiceBusResourceNames.CreateLinkRequestQueueKey)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinkRequestQueueName)
                .CreateAsync().ConfigureAwait(false);

            await ServiceBusListenerMock.AddQueueListenerAsync(createLinkRequestQueue.Name).ConfigureAwait(false);

            var createLinkReplyQueue = await ServiceBusResourceProvider
                .BuildQueue(ChargesClientsServiceBusResourceNames.CreateLinkReplyQueueKey)
                .SetEnvironmentVariableToQueueName(EnvironmentSettingNames.CreateLinkReplyQueueName)
                .CreateAsync().ConfigureAwait(false);

            await ServiceBusListenerMock.AddQueueListenerAsync(createLinkReplyQueue.Name).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeLibraryDependenciesAsync()
        {
            AzuriteManager.Dispose();
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);
        }
    }
}
