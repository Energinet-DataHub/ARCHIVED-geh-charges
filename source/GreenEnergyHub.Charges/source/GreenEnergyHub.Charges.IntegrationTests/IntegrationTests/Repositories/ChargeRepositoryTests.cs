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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using a database created with squadron.
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
        public async Task GetChargeAsync_WhenChargeIsCreated_ThenSuccessReturnedAsync()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await SeedDatabaseAsync(chargesDatabaseWriteContext).ConfigureAwait(false);
            var charge = GetValidCharge();
            var sut = new ChargeRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreChargeAsync(charge).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.Charges
                .Include(x => x.Points)
                .SingleOrDefaultAsync(x =>
                    x.Id == charge.Id &&
                    x.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                    x.OwnerId == charge.OwnerId &&
                    x.Type == charge.Type)
                .ConfigureAwait(false);

            actual.Should().BeEquivalentTo(charge);
            actual.Points.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CheckIfChargeExistsAsync_WhenChargeIsCreated_ThenSuccessReturnedAsync()
        {
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            // Arrange
            await SeedDatabaseAsync(chargesDatabaseWriteContext).ConfigureAwait(false);
            var charge = GetValidCharge();
            var sut = new ChargeRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreChargeAsync(charge)
                .ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual = chargesDatabaseReadContext.Charges.Any(x => x.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                                                                     x.OwnerId == charge.OwnerId &&
                                                                     x.Type == charge.Type);

            actual.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreChargeAsync_WhenChargeIsNull_ThrowsArgumentNullException(ChargeRepository sut)
        {
            // Arrange
            Charge? charge = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.StoreChargeAsync(charge!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetChargeAsync_WithId_ThenSuccessReturnedAsync()
        {
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();

            // Arrange
            var sut = new ChargeRepository(chargesDatabaseContext);
            var charge = GetValidCharge();
            await sut
                .StoreChargeAsync(charge)
                .ConfigureAwait(false);
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var createdCharge = chargesDatabaseReadContext.
                Charges.First(x => x.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                                                                     x.OwnerId == charge.OwnerId &&
                                                                     x.Type == charge.Type);

            // Act
            var actual = await sut.GetAsync(createdCharge.Id).ConfigureAwait(false);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task GetChargeAsync_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Arrange => Matching data from seeded test data
            var identifier = new ChargeIdentifier("EA-001", "5790000432752", ChargeType.Tariff);

            // Act
            var actual = await sut.GetAsync(identifier);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task GetChargesAsync_ReturnsCharges()
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Arrange => Matching data from seeded test data
            var firstCharge = await sut
                .GetAsync(
                    new ChargeIdentifier(
                        "EA-001",
                        "5790000432752",
                        ChargeType.Tariff))
                .ConfigureAwait(false);

            var secondCharge = await sut.
                GetAsync(
                    new ChargeIdentifier(
                        "45013",
                        "5790000432752",
                        ChargeType.Tariff))
                .ConfigureAwait(false);

            // Act
            var actual = await sut.GetAsync(new List<Guid>
            {
                firstCharge.Id,
                secondCharge.Id,
            });

            // Assert
            actual.Should().NotBeEmpty();
        }

        private static Charge GetValidCharge()
        {
            var charge = new Charge(
                Guid.NewGuid(),
                "SenderProvidedId",
                "Name",
                "description",
                _marketParticipantId,
                SystemClock.Instance.GetCurrentInstant(),
                Instant.FromUtc(9999, 12, 31, 23, 59, 59),
                ChargeType.Fee,
                VatClassification.Unknown,
                Resolution.P1D,
                true,
                false,
                new List<Point>
                {
                    new(0, 200m, SystemClock.Instance.GetCurrentInstant()),
                });

            return charge;
        }

        private static async Task SeedDatabaseAsync(ChargesDatabaseContext context)
        {
            var marketParticipant = await context
                .MarketParticipants
                .SingleOrDefaultAsync(x => x.MarketParticipantId == MarketParticipantOwnerId)
                .ConfigureAwait(false);

            if (marketParticipant != null)
                return;

            marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                MarketParticipantOwnerId,
                string.Empty,
                true,
                MarketParticipantRole.EnergySupplier);
            context.MarketParticipants.Add(marketParticipant);
            await context.SaveChangesAsync().ConfigureAwait(false);

            _marketParticipantId = marketParticipant.Id;
        }
    }
}
