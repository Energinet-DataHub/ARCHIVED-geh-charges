﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
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
            actual.Points.Should().NotBeNullOrEmpty();
            actual.Periods.Should().NotBeNullOrEmpty();
            actual.Points.Single().Price.Should().Be(200.111111m);
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
            Assert.NotNull(actual);
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
                new Guid("AF450C03-1937-4EA1-BB66-17B6E4AA51F5"),
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
                new Guid("AF450C03-1937-4EA1-BB66-17B6E4AA51F5"),
                ChargeType.Tariff));

            var secondCharge = await sut.SingleAsync(new ChargeIdentifier(
                "45013",
                new Guid("AF450C03-1937-4EA1-BB66-17B6E4AA51F5"),
                ChargeType.Tariff));

            // Act
            var actual = await sut.SingleAsync(new List<Guid>
            {
                firstCharge.Id,
                secondCharge.Id,
            });

            // Assert
            actual.First().OwnerId.Should().Be(firstCharge.OwnerId);
            actual.Last().OwnerId.Should().Be(secondCharge.OwnerId);
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
            var charge = new Charge(
                Guid.NewGuid(),
                $"ChgId{Guid.NewGuid().ToString("n")[..5]}",
                _marketParticipantId,
                ChargeType.Fee,
                Resolution.P1D,
                false,
                new List<Point> { new(1, 200.111111m, SystemClock.Instance.GetCurrentInstant()) },
                new List<ChargePeriod>
                {
                    new(
                        Guid.NewGuid(),
                        "Name",
                        "description",
                        VatClassification.Vat25,
                        true,
                        Instant.FromDateTimeUtc(DateTime.Now.Date.ToUniversalTime()),
                        InstantHelper.GetEndDefault()),
                });

            return charge;
        }

        private static async Task GetOrAddMarketParticipantAsync(ChargesDatabaseContext context)
        {
            var marketParticipant = await context
                .MarketParticipants
                .SingleOrDefaultAsync(x => x.MarketParticipantId == MarketParticipantOwnerId);

            if (marketParticipant != null)
                return;

            marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                MarketParticipantOwnerId,
                true,
                MarketParticipantRole.EnergySupplier);
            context.MarketParticipants.Add(marketParticipant);
            await context.SaveChangesAsync();

            _marketParticipantId = marketParticipant.Id;
        }
    }
}
