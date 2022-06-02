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
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    public class GridAreaLinkRepositoryTests
    {
        [IntegrationTest]
        public class GridAreaRepositoryTests : IClassFixture<ChargesDatabaseFixture>
        {
            private readonly ChargesDatabaseManager _databaseManager;

            public GridAreaRepositoryTests(ChargesDatabaseFixture fixture)
            {
                _databaseManager = fixture.DatabaseManager;
            }

            [Fact]
            public async Task GetOrNullAsync_ReturnsGridAreaLink()
            {
                // Arrange
                await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
                var sut = new GridAreaLinkRepository(chargesDatabaseContext);

                // Act
                var actual = await sut.GetOrNullAsync(SeededData.GridAreaLink.Provider8500000000013.Id);

                // Assert
                actual.Should().NotBeNull();
            }

            [Fact]
            public async Task GetGridAreaOrNullAsync_ReturnsGridAreaLink()
            {
                // Arrange
                await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
                var sut = new GridAreaLinkRepository(chargesDatabaseContext);

                // Act
                var actual = await sut.GetGridAreaOrNullAsync(SeededData.GridAreaLink.Provider8500000000013.GridAreaId);

                // Assert
                actual.Should().NotBeNull();
            }
        }
    }
}
