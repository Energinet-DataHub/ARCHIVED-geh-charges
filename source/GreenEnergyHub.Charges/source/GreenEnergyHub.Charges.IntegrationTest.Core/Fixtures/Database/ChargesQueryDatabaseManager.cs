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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database
{
    public class ChargesQueryDatabaseManager : SqlServerDatabaseManager<ChargesQueryDatabaseContext>
    {
        private readonly string? _connectionString;

        public ChargesQueryDatabaseManager(string? connectionString = null)
            : base("ChargesQuery")
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc/>
        public override ChargesQueryDatabaseContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChargesQueryDatabaseContext>()
                .UseSqlServer(_connectionString ?? ConnectionString, options => options.UseNodaTime());

            return new ChargesQueryDatabaseContext(optionsBuilder.Options);
        }
    }
}
