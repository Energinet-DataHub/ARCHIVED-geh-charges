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

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.Database;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="DefaultChargeLinkRepository"/> using an SQLite in-memory database.
    /// </summary>
    [IntegrationTest]
    public class DefaultChargeLinkRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public DefaultChargeLinkRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task GetDefaultChargeLinks_WhenCalledWithMeteringPointType_ReturnsDefaultCharges()
        {
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();

            // Arrange
            var sut = new DefaultChargeLinkRepository(chargesDatabaseContext);
            var x = chargesDatabaseContext.DefaultChargeLinks.ToList();

            // Act
            var actual = await
                sut.GetAsync(MeteringPointType.Consumption).ConfigureAwait(false);

            // Assert
            var actualDefaultChargeLinkSettings =
                actual as Charges.Domain.DefaultChargeLinks.DefaultChargeLink[] ?? actual.ToArray();

            actualDefaultChargeLinkSettings.Should().NotBeNullOrEmpty();
        }
    }
}
