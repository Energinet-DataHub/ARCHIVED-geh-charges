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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Polly;
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
        public async Task POC_SaveChangesAsync_WhenChargeWasJustInsertedByAnotherThread_ThrowsSqlException()
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

            // Simulating a simultaneous insert action for the same unique charge composite key, but new ID, caused by another request
            var sameChargeWithDiffId = builder
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("TariffId")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .Build();

            await using var anotherChargesDatabaseContext = _databaseManager.CreateDbContext();
            await anotherChargesDatabaseContext.Charges.AddAsync(sameChargeWithDiffId);
            await anotherChargesDatabaseContext.SaveChangesAsync();

            // Act / Assert
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => chargesDatabaseWriteContext.SaveChangesAsync());
            ex.InnerException.Should().BeOfType<SqlException>().Which.Message.Should().Contain(
                "Violation of UNIQUE KEY constraint 'UC_SenderProvidedChargeId_Type_OwnerId'. Cannot insert duplicate key in object 'Charges.Charge'");
        }

        [Fact]
        public async Task POC_UpdateAsync_WhenDbUpdateConcurrencyExceptionThrown_Handled()
        {
            // Arrange - Insert charge in storage
            await using var arrangeDbContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(arrangeDbContext);
            var arrangeRepo = new ChargeRepository(arrangeDbContext);

            var charge = new ChargeBuilder()
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("xCharge")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .WithPeriods(new List<ChargePeriod>
                {
                    new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .Build(),
                })
                .Build();

            await arrangeRepo.AddAsync(charge);
            await arrangeDbContext.SaveChangesAsync();

            // Set up SUT repo with new db context
            await using var testDbContext = _databaseManager.CreateDbContext();
            var sut = new ChargeRepository(testDbContext);
            var existingCharge = await sut.GetAsync(charge.Id);

            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithName("ResolvedConflictName")
                .Build();

            existingCharge.Update(newPeriod);
            await sut.UpdateAsync(existingCharge);

            // Simulating a concurrency conflict
            arrangeDbContext.Database.ExecuteSqlRaw(
                "UPDATE Charges.Charge SET SenderProvidedChargeId = 'yCharge' WHERE Id = '" + charge.Id + "';");

            // Act / Assert - and handled
            try
            {
                await testDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
                foreach (var entry in ex.Entries)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    foreach (var property in proposedValues.Properties)
                    {
                        var proposedValue = proposedValues[property];
                        _testOutputHelper.WriteLine("proposed value: " + proposedValue);
                        var databaseValue = databaseValues[property];
                        _testOutputHelper.WriteLine("database value: " + databaseValue);
                    }

                    testDbContext.ChangeTracker.Clear(); // Stop tracking entity
                    var updatedExistingCharge = await sut.GetAsync(charge.Id); // track again
                    updatedExistingCharge.Update(newPeriod);
                    await sut.UpdateAsync(updatedExistingCharge);
                    await testDbContext.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task SaveChanges_WhenDbUpdateConcurrencyExceptionThrown_HandledInSameDbContext()
        {
            // Arrange
            await using var arrangeDbContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(arrangeDbContext);
            var arrangeRepo = new ChargeRepository(arrangeDbContext);
            var period = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();

            var charge = new ChargeBuilder()
                .WithId(Guid.NewGuid())
                .WithSenderProvidedChargeId("TariffId3")
                .WithOwnerId(_marketParticipantId)
                .WithChargeType(ChargeType.Tariff)
                .WithPeriods(new List<ChargePeriod> { period })
                .Build();

            await arrangeRepo.AddAsync(charge);
            await arrangeDbContext.SaveChangesAsync();

            // Arrange SUT setup - Does not save changes immediately
            await using var testDbContext = _databaseManager.CreateDbContext();
            var sut = new ChargeRepository(testDbContext);
            var existingCharge =
                await testDbContext.Charges.FindAsync(charge.Id);

            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            existingCharge.Update(newPeriod);
            await sut.UpdateAsync(existingCharge);

            // Doing a simultaneous update of the same charge - to cause DbUpdateConcurrencyException on SUT.
            await using var anotherDbContext = _databaseManager.CreateDbContext();
            var anotherExistingCharge = await anotherDbContext.FindAsync<Charge>(charge.Id);

            var anotherNewPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();
            anotherExistingCharge.Update(anotherNewPeriod);
            anotherDbContext.Update(charge);
            await anotherDbContext.SaveChangesAsync();

            try
            {
                await testDbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Charge)entry.Entity;

                var dbEntry = await entry.GetDatabaseValuesAsync();
                var dbValues = (Charge)dbEntry.ToObject();
                // await testDbContext.Entry(existingCharge).ReloadAsync();
                // existingCharge.Update(newPeriod);
                // await sut.UpdateAsync(existingCharge);
                // await testDbContext.SaveChangesAsync();
            }

            // Assert
            await using var assertDbContext = _databaseManager.CreateDbContext();
            var assertRepo = new ChargeRepository(assertDbContext);
            var actual = await assertRepo.GetAsync(charge.Id);
            actual.Periods.Should().HaveCount(2);
            var firstPeriod = actual.Periods.OrderBy(p => p.StartDateTime).First();
            var lastPeriod = actual.Periods.OrderByDescending(p => p.StartDateTime).First();
            firstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            firstPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            lastPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            lastPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
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
