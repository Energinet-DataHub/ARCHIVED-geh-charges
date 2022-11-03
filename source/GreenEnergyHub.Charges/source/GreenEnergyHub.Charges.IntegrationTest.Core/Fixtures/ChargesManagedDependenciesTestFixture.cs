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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures
{
    /// <summary>
    /// A xUnit fixture for sharing managed dependencies for integration tests.
    ///
    /// This class ensures the following:
    ///  * All managed dependencies are created
    ///  * Integration test instances that uses the same fixture instance, uses the same database.
    ///  * The database is created similar to what we expect in a production environment (e.g. collation)
    ///  * Each fixture instance has an unique database instance (connection string).
    /// </summary>
    public sealed class ChargesManagedDependenciesTestFixture : IAsyncLifetime
    {
        public ChargesManagedDependenciesTestFixture()
        {
            DatabaseManager = new ChargesDatabaseManager();

            var configuration = new FunctionAppHostConfigurationBuilder().BuildLocalSettingsConfiguration();
            FunctionHostEnvironmentSettingHelper.SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(configuration);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.ChargeDbConnectionString, DatabaseManager.ConnectionString);
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.LocalTimeZoneName, "Europe/Copenhagen");

            Host = ChargesFunctionApp.ConfigureApplication();

            UnitOfWork = GetService<IUnitOfWork>();
            CorrelationContext = GetService<ICorrelationContext>();
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        public IUnitOfWork UnitOfWork { get; }

        public ICorrelationContext CorrelationContext { get; }

        private IHost Host { get; }

        public Task InitializeAsync()
        {
            return DatabaseManager.CreateDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return DatabaseManager.DeleteDatabaseAsync();
        }

        public T GetService<T>()
        {
            return (T)Host.Services.GetService(typeof(T))!;
        }
    }
}
