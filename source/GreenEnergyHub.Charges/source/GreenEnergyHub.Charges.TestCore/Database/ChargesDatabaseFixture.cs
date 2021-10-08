﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Threading.Tasks;
using Xunit;

namespace GreenEnergyHub.Charges.TestCore.Database
{
    /// <summary>
    /// A xUnit fixture for sharing a Charges database for integration tests.
    ///
    /// This class ensures the following:
    ///  * Integration test instances that uses the same fixture instance, uses the same database.
    ///  * The database is created similar to what we expect in a production environment (e.g. collation)
    ///  * Each fixture instance has an unique database instance (connection string).
    /// </summary>
    public sealed class ChargesDatabaseFixture : IAsyncLifetime
    {
        public ChargesDatabaseFixture()
        {
            DatabaseManager = new ChargesDatabaseManager();
        }

        public ChargesDatabaseManager DatabaseManager { get; }

        public Task InitializeAsync()
        {
            return DatabaseManager.CreateDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return DatabaseManager.DeleteDatabaseAsync();
        }
    }
}
