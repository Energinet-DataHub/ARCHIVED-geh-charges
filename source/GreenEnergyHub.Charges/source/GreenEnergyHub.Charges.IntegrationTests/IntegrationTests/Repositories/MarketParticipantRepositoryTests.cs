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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="MarketParticipantRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class MarketParticipantRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public MarketParticipantRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task AddAsync_WhenValidMarketParticipant_IsPersisted()
        {
            // Arrange
            await using var chargesWriteDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesWriteDatabaseContext);
            var id = Guid.NewGuid();
            var marketParticipant = new MarketParticipant(id, "00001", true, MarketParticipantRole.GridAccessProvider);

            // Act
            await sut.AddAsync(marketParticipant).ConfigureAwait(false);
            await chargesWriteDatabaseContext.SaveChangesAsync();

            // Assert
            await using var chargesReadDatabaseContext = _databaseManager.CreateDbContext();
            var actual =
                await chargesReadDatabaseContext.MarketParticipants.SingleAsync(mp => mp.Id == marketParticipant.Id);
            actual.Id.Should().Be(id);
            actual.MarketParticipantId.Should().Be("00001");
            actual.IsActive.Should().BeTrue();
            actual.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenGuidEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);
            var existingMarketParticipant = await chargesDatabaseContext.MarketParticipants
                .SingleAsync(mp => mp.MarketParticipantId == SeededData.MarketParticipant.Inactive8900000000005);

            // Act
            var actual = await sut.SingleOrNullAsync(existingMarketParticipant.Id);

            // Assert
            actual!.MarketParticipantId.Should().Be(SeededData.MarketParticipant.Inactive8900000000005);
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenMarketParticipantIdEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.SingleOrNullAsync(SeededData.MarketParticipant.Inactive8900000000005);

            // Assert
            actual.Should().NotBeNull();
            actual!.MarketParticipantId.Should().Be(SeededData.MarketParticipant.Inactive8900000000005);
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenRoleAndMarketParticipantIdEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var writeContext = _databaseManager.CreateDbContext();
            await AddMarketParticipantToContextAsync("1337", MarketParticipantRole.GridAccessProvider, writeContext);
            await AddMarketParticipantToContextAsync("1337", MarketParticipantRole.EnergySupplier, writeContext);
            await writeContext.SaveChangesAsync();

            await using var readContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(readContext);

            // Act
            var actual = await sut.SingleOrNullAsync(MarketParticipantRole.GridAccessProvider, "1337");

            // Assert
            actual.Should().NotBeNull();
            actual!.MarketParticipantId.Should().Be("1337");
            actual!.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
        }

        [Fact]
        public async Task GetGridAccessProviderAsync_WhenNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistingMeteringPointId = Guid.NewGuid().ToString();
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act and assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await sut.GetGridAccessProviderAsync(nonExistingMeteringPointId));
        }

        [Fact]
        public async Task GetGridAccessProviderAsync_ReturnsGridAccessProvider()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetGridAccessProviderAsync(SeededData.MeteringPoints.Mp571313180000000005.Id);

            // Assert
            actual.MarketParticipantId.Should().Be(SeededData.MeteringPoints.Mp571313180000000005.GridAccessProvider);
            actual.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
            actual.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveGridAccessProvidersAsync()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetGridAccessProvidersAsync();

            // Assert
            actual.Should().NotContain(x => x.MarketParticipantId == SeededData.MarketParticipant.Inactive8900000000005);
        }

        [Fact]
        public async Task GetGridAccessProvidersAsync_WhenIsActive_ReturnsListWithActiveGridAccessProvider()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetGridAccessProvidersAsync();

            // Assert
            actual.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetGridAccessProviderAsync_ReturnsActiveGridAccessProvider()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetGridAccessProviderAsync(SeededData.MeteringPoints.Mp571313180000000005.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetMeteringPointAdministratorAsync_ReturnsActiveMeteringPointAdministrator()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetMeteringPointAdministratorAsync();

            // Assert
            actual.Should().NotBeNull();
            actual.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetSystemOperatorAsync_ReturnsActiveSystemOperator()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetSystemOperatorAsync();

            // Assert
            actual.Should().NotBeNull();
            actual.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetGridAccessProviderAsync_ReturnsGridAccessProviderFromGridAreaId()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetGridAccessProviderAsync(SeededData.GridAreaLink.Provider8100000000030.GridAreaId);

            // Assert
            actual.Should().NotBeNull();
            actual?.MarketParticipantId.Should().Be(SeededData.GridAreaLink.Provider8100000000030.MarketParticipantId);
        }

        private static async Task AddMarketParticipantToContextAsync(string marketParticipantId, MarketParticipantRole role, ChargesDatabaseContext context)
        {
            var marketParticipant = new MarketParticipant(Guid.NewGuid(), marketParticipantId, true, role);
            await context.AddAsync(marketParticipant);
            await context.SaveChangesAsync();
        }
    }
}
