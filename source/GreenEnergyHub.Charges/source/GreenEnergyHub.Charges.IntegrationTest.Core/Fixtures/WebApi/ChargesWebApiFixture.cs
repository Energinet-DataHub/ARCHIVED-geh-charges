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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.WebApi;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi
{
    public class ChargesWebApiFixture : WebApiFixture
    {
        public ChargesWebApiFixture()
        {
            DatabaseManager = new ChargesDatabaseManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            AuthorizationConfiguration =
                AuthorizationConfigurationData.CreateAuthorizationConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(
                IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        public B2CAuthorizationConfiguration AuthorizationConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        [NotNull]
        private TopicResource? ChargesDomainEventTopic { get; set; }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeWebApiDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            // => Database
            await DatabaseManager.CreateDatabaseAsync();

            // Overwrites the setting so the Web Api app uses the database we have control of in the test
            Environment.SetEnvironmentVariable(
                $"CONNECTIONSTRINGS:{EnvironmentSettingNames.ChargeDbConnectionString}",
                DatabaseManager.ConnectionString);

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.FrontEndOpenIdUrl, AuthorizationConfiguration.FrontendOpenIdConfigurationUrl);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.FrontEndServiceAppId, AuthorizationConfiguration.FrontendApp.AppId);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, "Europe/Copenhagen");

            Environment.SetEnvironmentVariable(EnvironmentSettingNames.DataHubSenderConnectionString, ServiceBusResourceProvider.ConnectionString);

            ChargesDomainEventTopic = await ServiceBusResourceProvider
                .BuildTopic(ChargesServiceBusResourceNames.ChargesDomainEventsTopicKey)
                .SetEnvironmentVariableToTopicName(EnvironmentSettingNames.ChargesDomainEventTopicName).CreateAsync();
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeWebApiDependenciesAsync()
        {
            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync();

            // => Database
            await DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
