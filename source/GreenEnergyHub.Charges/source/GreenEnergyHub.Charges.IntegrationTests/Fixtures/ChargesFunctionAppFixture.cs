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
using GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.TestCore.Squadron;
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

            ServiceBusResource = new AzureCloudServiceBusResource<ChargesFunctionAppServiceBusOptions>(messageSink);
            SqlServerResource = new SqlServerResource<SqlServerOptions>();
        }

        [NotNull]
        public ServiceBusListenerMock? ServiceBusListenerMock { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private AzureCloudServiceBusResource<ChargesFunctionAppServiceBusOptions> ServiceBusResource { get; }

        private SqlServerResource<SqlServerOptions> SqlServerResource { get; }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            // => Service Bus
            await ServiceBusResource.InitializeAsync();

            var postOfficeTopicName = await GetTopicNameFromKeyAsync(ChargesFunctionAppServiceBusOptions.PostOfficeTopicKey);

            // Overwrites the setting so the function uses the name we have control of in the test
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.PostOfficeTopicName, postOfficeTopicName);

            ServiceBusListenerMock = new ServiceBusListenerMock(ServiceBusResource.ConnectionString, TestLogger);
            await ServiceBusListenerMock.AddTopicSubscriptionListenerAsync(postOfficeTopicName, ChargesFunctionAppServiceBusOptions.PostOfficeTopicSubscriptionName);

            // => Database
            await SqlServerResource.InitializeAsync();

            const string databaseName = "chargeDatabase";
            var chargeDbConnectionString = await SqlServerResource.CreateDatabaseAsync(databaseName);

            // Overwrites the setting so the function uses the name we have control of in the test
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeDbConnectionString, chargeDbConnectionString);

            var upgrader = UpgradeFactory.GetUpgradeEngine(chargeDbConnectionString, _ => true);
            var result = upgrader.PerformUpgrade();
            if (result.Successful is false)
            {
                throw new Exception("Database migration failed", result.Error);
            }
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
            await SqlServerResource.DisposeAsync();
        }

        protected async Task<string> GetTopicNameFromKeyAsync(string topicKey)
        {
            var topicClient = ServiceBusResource.GetTopicClient(topicKey);
            var topicName = topicClient.TopicName;
            await topicClient.CloseAsync();

            return topicName;
        }
    }
}
