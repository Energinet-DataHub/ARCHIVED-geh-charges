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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
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
            var actorId = Guid.NewGuid();
            var b2CActorId = Guid.NewGuid();
            var marketParticipant = new MarketParticipant(
                id, actorId, b2CActorId, "00001", MarketParticipantStatus.Active, MarketParticipantRole.GridAccessProvider);

            // Act
            await sut.AddAsync(marketParticipant).ConfigureAwait(false);
            await chargesWriteDatabaseContext.SaveChangesAsync();

            // Assert
            await using var chargesReadDatabaseContext = _databaseManager.CreateDbContext();
            var actual = await chargesReadDatabaseContext.MarketParticipants
                .SingleAsync(mp => mp.Id == marketParticipant.Id);
            actual.Id.Should().Be(id);
            actual.ActorId.Should().Be(actorId);
            actual.B2CActorId.Should().Be(b2CActorId);
            actual.MarketParticipantId.Should().Be("00001");
            actual.Status.Should().Be(MarketParticipantStatus.Active);
            actual.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
        }

        [Fact]
        public async Task AddAsync_WhenMarketParticipantIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(null!));
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenB2CActorIdEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);
            var existingMarketParticipant = await chargesDatabaseContext.MarketParticipants.SingleAsync(mp =>
                    mp.MarketParticipantId == SeededData.MarketParticipants.Inactive8900000000005.Gln);

            // Act
            var actual = await sut.SingleOrNullAsync(existingMarketParticipant.B2CActorId ?? Guid.NewGuid());

            // Assert
            actual!.MarketParticipantId.Should().Be(SeededData.MarketParticipants.Inactive8900000000005.Gln);
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenMarketParticipantIdEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.SingleOrNullAsync(SeededData.MarketParticipants.Inactive8900000000005.Gln);

            // Assert
            actual.Should().NotBeNull();
            actual!.MarketParticipantId.Should().Be(SeededData.MarketParticipants.Inactive8900000000005.Gln);
        }

        [Fact]
        public async Task SingleOrNullAsync_WhenRoleAndMarketParticipantIdEqualsExistingMarketParticipant_ReturnsMarketParticipant()
        {
            // Arrange
            await using var writeDatabaseContext = _databaseManager.CreateDbContext();
            await AddMarketParticipantToContextAndSaveAsync("1337", MarketParticipantRole.GridAccessProvider, MarketParticipantStatus.Active, writeDatabaseContext);
            await AddMarketParticipantToContextAndSaveAsync("1337", MarketParticipantRole.EnergySupplier, MarketParticipantStatus.Active, writeDatabaseContext);

            await using var readDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(readDatabaseContext);

            // Act
            var actual = await sut.SingleOrNullAsync(MarketParticipantRole.GridAccessProvider, "1337");

            // Assert
            actual.Should().NotBeNull();
            actual!.MarketParticipantId.Should().Be("1337");
            actual.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
        }

        [Fact]
        public async Task GetAsync_ListOfExistingMarketParticipantIds_ReturnsMarketParticipants()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);
            var idsOnExistingMarketParticipants = new List<Guid>()
            {
                Guid.Parse("75ED087C-5A15-4D30-8711-FCD509A8D559"),
                Guid.Parse("369F2216-2237-454A-9C4F-BAB34726BFE4"),
            };

            // Act
            var actualMarketParticipants = await sut.GetAsync(idsOnExistingMarketParticipants);

            // Arrange
            actualMarketParticipants.Count.Should().Be(2);
            var firstMarketParticipant = actualMarketParticipants.First();
            firstMarketParticipant.MarketParticipantId.Should().Be("8100000000016");
            var secondMarketParticipant = actualMarketParticipants.Last();
            secondMarketParticipant.MarketParticipantId.Should().Be("8100000000023");
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
        public async Task GetGridAccessProviderAsync_WhenEmpty_ThrowsArgumentException()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act and assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await sut.GetGridAccessProviderAsync(string.Empty));
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
            actual.Status.Should().Be(MarketParticipantStatus.Active);
        }

        [Fact]
        public async Task GetActiveGridAccessProvidersAsync()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetActiveAndPassiveGridAccessProvidersAsync();

            // Assert
            actual.Should().NotContain(x => x.MarketParticipantId == SeededData.MarketParticipants.Inactive8900000000005.Gln);
        }

        [Fact]
        public async Task GetGridAccessProvidersAsync_WhenIsActive_ReturnsListWithActiveGridAccessProvider()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(chargesDatabaseContext);

            // Act
            var actual = await sut.GetActiveAndPassiveGridAccessProvidersAsync();

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
            actual.Status.Should().Be(MarketParticipantStatus.Active);
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
            actual.Status.Should().Be(MarketParticipantStatus.Active);
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
            actual.Status.Should().Be(MarketParticipantStatus.Active);
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

        [Fact]
        public async Task GetSystemOperatorOrGridAccessProviderAsync_WhenSharedGlnAndGridAccessProviderIsNotActive_ReturnsSystemOperator()
        {
            // Arrange
            await using var writeDatabaseContext = _databaseManager.CreateDbContext();
            const string marketParticipantId = SeededData.MarketParticipants.SystemOperator.Gln;
            await AddMarketParticipantToContextAndSaveAsync(
                marketParticipantId,
                MarketParticipantRole.GridAccessProvider,
                MarketParticipantStatus.Inactive,
                writeDatabaseContext);

            await using var readDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(readDatabaseContext);

            // Act
            var actual = await sut.GetSystemOperatorOrGridAccessProviderAsync(marketParticipantId);

            // Assert
            actual.BusinessProcessRole.Should().Be(MarketParticipantRole.SystemOperator);
            actual.Status.Should().Be(MarketParticipantStatus.Active);
        }

        [Fact]
        public async Task GetActiveAndPassiveEnergySuppliersAsync_WhenCalled_ShouldHaveExpected()
        {
            // Arrange
            await using var readDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(readDatabaseContext);

            // Act
            var actual = await sut.GetActiveAndPassiveEnergySuppliersAsync().ConfigureAwait(false);

            // Assert
            actual.Should().Contain(mp =>
                mp.MarketParticipantId == "8100000001002" && mp.Status == MarketParticipantStatus.Active);
            actual.Should().Contain(mp =>
                mp.MarketParticipantId == "8100000001004" && mp.Status == MarketParticipantStatus.Passive);
            actual.Should().NotContain(mp =>
                mp.MarketParticipantId == "8100000001001" ||
                mp.MarketParticipantId == "8100000001003" ||
                mp.MarketParticipantId == "8100000001005");
        }

        [Fact]
        public async Task GetActiveAndPassiveGridAccessProvidersAsync_WhenCalled_ShouldHaveExpected()
        {
            // Arrange
            await using var readDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new MarketParticipantRepository(readDatabaseContext);

            // Act
            var actual = await sut.GetActiveAndPassiveGridAccessProvidersAsync().ConfigureAwait(false);

            // Assert
            actual.Should().Contain(mp =>
                mp.MarketParticipantId == "8100000002002" && mp.Status == MarketParticipantStatus.Active);
            actual.Should().Contain(mp =>
                mp.MarketParticipantId == "8100000002004" && mp.Status == MarketParticipantStatus.Passive);
            actual.Should().NotContain(mp =>
                mp.MarketParticipantId == "8100000002001" ||
                mp.MarketParticipantId == "8100000002003" ||
                mp.MarketParticipantId == "8100000002005");
        }

        private static async Task AddMarketParticipantToContextAndSaveAsync(
            string marketParticipantId, MarketParticipantRole role, MarketParticipantStatus status, ChargesDatabaseContext context)
        {
            var marketParticipant = new MarketParticipant(
                id: Guid.NewGuid(),
                actorId: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                marketParticipantId,
                status,
                role);
            await context.AddAsync(marketParticipant);
            await context.SaveChangesAsync();
        }
    }
}
