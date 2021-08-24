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

using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
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
            var validMeteringPoint = GetMeteringPointCreatedEvent();
            var sut = new MeteringPointRepository(chargesDatabaseContext);

            // Act
            await sut.StoreMeteringPointCreatedEventAsync(validMeteringPoint).ConfigureAwait(false);

            // Assert
            var expected = await sut.GetMeteringPointAsync(validMeteringPoint.MeteringPointId).ConfigureAwait(false);
            expected.Id.Should().BeGreaterThan(0);
            expected.ConnectionState.Should().Be(int.Parse(validMeteringPoint.ConnectionState, CultureInfo.InvariantCulture));
            expected.MeteringGridArea.Should().Be(validMeteringPoint.GridArea);
            expected.MeteringPointId.Should().Be(validMeteringPoint.MeteringPointId);
            expected.EffectiveDate.Should().Be(InstantPattern.General.Parse(validMeteringPoint.EffectiveDate).Value);
            expected.SettlementMethod.Should().Be(int.Parse(validMeteringPoint.SettlementMethod, CultureInfo.InvariantCulture));
        }

        private static MeteringPointCreatedEvent GetMeteringPointCreatedEvent()
        {
            return new MeteringPointCreatedEvent(
                "123",
                MeteringPointType.Consumption.ToString(),
                "234",
                "2",
                "1",
                "2",
                "mrp",
                "456",
                "567",
                "678",
                "product",
                "1",
                SystemClock.Instance.GetCurrentInstant().ToString());
        }

        private void EnsureDatabaseCreated()
        {
            using var context = new ChargesDatabaseContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
