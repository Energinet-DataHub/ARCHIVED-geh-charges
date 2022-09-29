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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class ChargeRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private const string MarketParticipantOwnerId = "MarketParticipantId";

        // Is being set when executing the SeedDatabase method
        private static Guid _marketParticipantId;

        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task AddAsync_WhenChargeIsValid_ChargeIsAdded()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);
            var charge = GetValidCharge();
            var sut = new ChargeRepository(chargesDatabaseWriteContext);

            // Act
            await sut.AddAsync(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.Charges
                .SingleAsync(x =>
                    x.Id == charge.Id &&
                    x.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                    x.OwnerId == charge.OwnerId &&
                    x.Type == charge.Type);

            actual.Should().BeEquivalentTo(charge);
            actual.Points.Should().BeEmpty();
            actual.Periods.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddAsync_WhenChargeIsNull_ThrowsArgumentNullException(ChargeRepository sut)
        {
            // Arrange
            Charge? charge = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(charge!));
        }

        [Fact]
        public async Task FindAsync_WithKnownId_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var charge = new ChargeBuilder().Build();
            var marketParticipant = new MarketParticipantBuilder().WithId(charge.OwnerId).Build();
            await chargesDatabaseContext.MarketParticipants.AddAsync(marketParticipant);
            await chargesDatabaseContext.SaveChangesAsync();
            await chargesDatabaseContext.AddAsync(charge);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act
            var key = new ChargeIdentifier(
                charge.SenderProvidedChargeId,
                charge.OwnerId,
                charge.Type);
            var actual = await sut.SingleAsync(key);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task FindAsync_WithChargeIdentifier_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseContext);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Arrange => Matching data from seeded test data
            var identifier = new ChargeIdentifier(
                "EA-001",
                SeededData.MarketParticipants.SystemOperator.Id,
                ChargeType.Tariff);

            // Act
            var actual = await sut.SingleAsync(identifier);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task FindAsync_ReturnsCharges()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            await SetupValidChargeAsync(chargesDatabaseContext);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Arrange => Matching data from seeded test data
            var firstCharge = await sut.SingleAsync(new ChargeIdentifier(
                "EA-001",
                SeededData.MarketParticipants.SystemOperator.Id,
                ChargeType.Tariff));

            var secondCharge = await sut.SingleAsync(new ChargeIdentifier(
                "45013",
                SeededData.MarketParticipants.SystemOperator.Id,
                ChargeType.Tariff));

            // Act
            var actual = await sut.GetByIdsAsync(new List<Guid>
            {
                firstCharge.Id,
                secondCharge.Id,
            });

            // Assert
            actual.First().OwnerId.Should().Be(firstCharge.OwnerId);
            actual.Last().OwnerId.Should().Be(secondCharge.OwnerId);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetByIdsAsync_VerifyLocalContextItemsAreAlsoFetched(ChargeBuilder chargeBuilder)
        {
            // Arrange
            var notSavedCharge = chargeBuilder.Build();
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var savedCharge = await SetupValidChargeAsync(chargesDatabaseContext);
            var sut = new ChargeRepository(chargesDatabaseContext);
            await sut.AddAsync(notSavedCharge);

            // Act
            var actual = await sut.GetByIdsAsync(new List<Guid> { savedCharge.Id, notSavedCharge.Id });

            // Assert
            actual.Count.Should().Be(2);
            actual.Should().Contain(savedCharge);
            actual.Should().Contain(notSavedCharge);
            chargesDatabaseContext.Entry(savedCharge).State.Should().Be(EntityState.Unchanged);
            chargesDatabaseContext.Entry(notSavedCharge).State.Should().Be(EntityState.Added);
        }

        private static async Task<Charge> SetupValidChargeAsync(ChargesDatabaseContext chargesDatabaseWriteContext)
        {
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);
            var charge = GetValidCharge();
            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            return charge;
        }

        private static Charge GetValidCharge()
        {
            return new ChargeBuilder()
                .WithStartDate(InstantHelper.GetTodayAtMidnightUtc())
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..5]}")
                .WithOwnerId(_marketParticipantId)
                .Build();
        }

        private static async Task GetOrAddMarketParticipantAsync(ChargesDatabaseContext context)
        {
            var marketParticipant = await context
                .MarketParticipants
                .SingleOrDefaultAsync(x => x.MarketParticipantId == MarketParticipantOwnerId);

            if (marketParticipant != null)
                return;

            marketParticipant = new MarketParticipant(
                id: Guid.NewGuid(),
                actorId: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                MarketParticipantOwnerId,
                MarketParticipantStatus.Active,
                MarketParticipantRole.EnergySupplier);
            context.MarketParticipants.Add(marketParticipant);
            await context.SaveChangesAsync();

            _marketParticipantId = marketParticipant.Id;
        }
    }
}
