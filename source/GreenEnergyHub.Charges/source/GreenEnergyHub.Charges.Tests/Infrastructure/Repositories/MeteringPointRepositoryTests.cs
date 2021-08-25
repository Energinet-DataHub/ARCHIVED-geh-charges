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

using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    public class MeteringPointRepositoryTests
    {
        private readonly DbContextOptions<ChargesDatabaseContext> _dbContextOptions =
            new DbContextOptionsBuilder<ChargesDatabaseContext>()
                .UseSqlite("Filename=Test.db")
                .Options;

        [Fact]
        public async Task StoreMeteringPointAsync_WhenMeteringPointIsCreated_StoresMeteringPointInDatabase()
        {
            // Arrange
            EnsureDatabaseCreated();
            await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
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

        private void EnsureDatabaseCreated()
        {
            using var context = new ChargesDatabaseContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
