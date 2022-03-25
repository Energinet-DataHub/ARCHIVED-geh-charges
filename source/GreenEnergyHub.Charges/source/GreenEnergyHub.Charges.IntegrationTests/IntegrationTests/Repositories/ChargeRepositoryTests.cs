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
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
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
        private readonly ITestOutputHelper _testOutputHelper;

        // Is being set when executing the SeedDatabase method
        private static Guid _marketParticipantId;

        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeRepositoryTests(ChargesDatabaseFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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
                .SingleOrDefaultAsync(x =>
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
        public async Task GetAsync_WithId_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var charge = await SetupValidCharge(chargesDatabaseWriteContext);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new ChargeRepository(chargesDatabaseReadContext);

            // Act
            var actual = await sut.GetAsync(charge.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeEquivalentTo(charge);
        }

        [Fact]
        public async Task GetAsync_WithChargeIdentifier_ReturnsCharge()
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
            var firstCharge = await sut.GetAsync(
                    new ChargeIdentifier("EA-001", "5790000432752", ChargeType.Tariff));

            var secondCharge = await sut.GetAsync(
                new ChargeIdentifier("45013", "5790000432752", ChargeType.Tariff));

            // Act
            var actual = await sut.GetAsync(new List<Guid>
            {
                firstCharge.Id,
                secondCharge.Id,
            });

            // Assert
            actual.Should().NotBeEmpty();
        }

        [Fact]
        public async Task WIP_AddAsync_WhenChargeWasJustInsertedByAnotherThread_Throws()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);
            var sut = new ChargeRepository(chargesDatabaseWriteContext);
            var builder = new ChargeBuilder();

            var charge = builder
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("TariffId")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .Build();

            await sut.AddAsync(charge);

            // Simulating a simultaneous insert action for the same unique charge caused by another operation or request
            var sameChargeWithDiffId = builder
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("TariffId")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .Build();

            await using var anotherChargesDatabaseContext = _databaseManager.CreateDbContext();
            await anotherChargesDatabaseContext.Charges.AddAsync(sameChargeWithDiffId);
            await anotherChargesDatabaseContext.SaveChangesAsync();

            try
            {
                await chargesDatabaseWriteContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
                throw;
            }

            // Act / Assert
            // var ex = await Assert.ThrowsAsync<DbUpdateException>(() => chargesDatabaseWriteContext.SaveChangesAsync());
            // ex.InnerException.Should().BeOfType<SqlException>().Which.Message.Should().Contain("Violation of UNIQUE KEY constraint 'UC_SenderProvidedChargeId_Type_OwnerId'. Cannot insert duplicate key in object 'Charges.Charge'");
        }

        [Fact]
        public async Task WIP_AddAsync_WhenUpdatingChargeSimultaneously_Throws()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);
            var arrangeRepo = new ChargeRepository(chargesDatabaseWriteContext);
            var period = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();

            var charge = new ChargeBuilder()
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("TariffId")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .WithPeriods(new List<ChargePeriod> { period })
                .Build();

            await arrangeRepo.AddAsync(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Thread one; doing an update doesnt SaveChanges before thread two
            await using var threadOneChargesDbContext = _databaseManager.CreateDbContext();
            var threadOneRepo = new ChargeRepository(threadOneChargesDbContext);
            var existingCharge = await threadOneRepo.GetAsync(charge.Id);

            var threadOneNewPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            existingCharge.Update(threadOneNewPeriod);
            await threadOneRepo.UpdateAsync(existingCharge);

            // Thread two doing an update - SaveChanges before thread one
            await using var threadTwoChargesDbContext = _databaseManager.CreateDbContext();
            var threadTwoRepo = new ChargeRepository(threadTwoChargesDbContext);
            var threadTwoExistingCharge = await threadTwoRepo.GetAsync(charge.Id);

            var threadTwoNewPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            threadTwoExistingCharge.Update(threadTwoNewPeriod);
            await threadTwoRepo.UpdateAsync(threadTwoExistingCharge);
            await threadTwoChargesDbContext.SaveChangesAsync();

            try
            {
                await threadOneChargesDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());

                // foreach (var entry in ex.Entries)
                // {
                //     var dbValues = await entry.GetDatabaseValuesAsync();
                //     entry.OriginalValues.SetValues(dbValues);
                // }
                var threadTwoUpdatedCharge = await threadTwoRepo.GetAsync(charge.Id);
                threadTwoUpdatedCharge.Update(threadTwoNewPeriod);
                await threadTwoRepo.UpdateAsync(threadTwoUpdatedCharge);
                await threadTwoChargesDbContext.SaveChangesAsync();
            }

            var resultingCharge = await threadOneRepo.GetAsync(charge.Id);
        }

        private static async Task<Charge> SetupValidCharge(ChargesDatabaseContext chargesDatabaseWriteContext)
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
                "SenderProvidedId",
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
