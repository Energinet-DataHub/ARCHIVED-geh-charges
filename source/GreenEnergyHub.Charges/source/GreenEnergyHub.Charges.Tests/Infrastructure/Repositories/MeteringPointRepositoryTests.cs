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
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Squadron;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Squadron;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    [UnitTest]
    public class MeteringPointRepositoryTests : IClassFixture<SqlServerResource<SqlServerOptions>>
    {
        private readonly SqlServerResource<SqlServerOptions> _resource;

        public MeteringPointRepositoryTests(SqlServerResource<SqlServerOptions> resource)
        {
            _resource = resource;
        }

        [Fact]
        public async Task StoreMeteringPointAsync_WhenMeteringPointIsCreated_StoresMeteringPointInDatabase()
        {
            // Arrange
            await using var chargesDatabaseContext = await GetDatabaseContext(_resource)
                .ConfigureAwait(false);
            {
                var expected = GetMeteringPointCreatedEvent();
                var sut = new MeteringPointRepository(chargesDatabaseContext);

                // Act
                await sut.StoreMeteringPointAsync(expected).ConfigureAwait(false);

                // Assert
                var actual = await sut.GetMeteringPointAsync(expected.MeteringPointId).ConfigureAwait(false);
                actual.ConnectionState.Should().Be(expected.ConnectionState);
                actual.GridAreaId.Should().Be(expected.GridAreaId);
                actual.MeteringPointId.Should().Be(expected.MeteringPointId);
                actual.EffectiveDate.Should().Be(expected.EffectiveDate);
                actual.SettlementMethod.Should().Be(expected.SettlementMethod);
            }
        }

        private static MeteringPoint GetMeteringPointCreatedEvent()
        {
            return new MeteringPoint(
                "123",
                MeteringPointType.Consumption,
                "234",
                SystemClock.Instance.GetCurrentInstant(),
                ConnectionState.Connected,
                SettlementMethod.Flex);
        }

        private static async Task<ChargesDatabaseContext> GetDatabaseContext(SqlServerResource<SqlServerOptions> resource)
        {
            var connectionString = await CreateDatabaseAsync(resource)
                .ConfigureAwait(false);
            DbContextOptions<ChargesDatabaseContext> dbContextOptions =
                new DbContextOptionsBuilder<ChargesDatabaseContext>()
                    .UseSqlServer(connectionString)
                    .Options;

            var chargesContext = new ChargesDatabaseContext(dbContextOptions);

            return chargesContext;
        }

        private static async Task<string> CreateDatabaseAsync(SqlServerResource<SqlServerOptions> resource)
        {
            const string databaseName = "New_Database";

            var connectionString = await resource.CreateDatabaseAsync(databaseName).ConfigureAwait(false);

            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, _ => true);
            var result = upgrader.PerformUpgrade();

            if (result.Successful is false)
            {
                throw new Exception("Database migration failed");
            }

            return connectionString;
        }
    }
}
