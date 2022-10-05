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
using Energinet.Charges.Contracts.Charge;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.TestHelpers;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;
using ChargePeriod = GreenEnergyHub.Charges.Domain.Charges.ChargePeriod;
using ChargePeriodBuilder = GreenEnergyHub.Charges.TestCore.Builders.Command.ChargePeriodBuilder;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargesQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private const string MarketParticipantOwnerId = "MarketParticipantId";

        private static Guid _marketParticipantId;

        private readonly ChargesDatabaseManager _databaseManager;

        public ChargesQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task SearchAsync_WhenNoSearchCriteria_ReturnsAllCharges()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder().Build();

            var expected = GetActiveCharges(chargesDatabaseQueryContext).Count();
            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeId_ReturnsOneCharge()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = charge.SenderProvidedChargeId;

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeIdOrName(expected)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(1);
            actual.Single().ChargeId.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Subscription, ChargeType.D01)]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Fee, ChargeType.D02)]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Tariff, ChargeType.D03)]
        public async Task SearchAsync_WhenSearchingByChargeType_ReturnsChargesWithChargeType(
            Domain.Charges.ChargeType chargeType,
            ChargeType expectedChargeType)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);
            var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime().ToInstant();

            var charge = GetValidCharge(chargeType);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = chargesDatabaseWriteContext.Charges.Count(c => c.Type == chargeType
            && c.Periods.Any(p => p.StartDateTime <= todayAtMidnightUtc));

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeType(expectedChargeType)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Should().Contain(c => c.ChargeType == expectedChargeType);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByMultipleChargeTypes_ReturnsChargesWithChargeTypes()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = chargesDatabaseWriteContext.Charges.Count(c =>
                c.Type == Domain.Charges.ChargeType.Subscription || c.Type == Domain.Charges.ChargeType.Fee);

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeTypes(new List<ChargeType> { ChargeType.D01, ChargeType.D02 })
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Should().Contain(c => c.ChargeType == ChargeType.D01 || c.ChargeType == ChargeType.D02);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeName_ReturnsOneCharge_WithOnePeriod()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = charge.Periods.First().Name;

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeIdOrName(expected)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(1);
            actual.Single().ChargeName.Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeName_ReturnsOneCharge_WithMultiplePeriods()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var expected = "test_with_many_periods_get_one";
            var charge = GetValidChargeWithMultiplePeriods(expected);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeIdOrName(expected)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(1);
            actual.Single().ChargeName.Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeNameButOnlyPartOfIt_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var searchName = charge.Periods.First().Name[..5];
            var expected = charge.SenderProvidedChargeId;

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithChargeIdOrName(searchName)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(1);
            actual.Single().ChargeId.Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByMarketParticipantId_ReturnsCharge()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetValidCharge();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = chargesDatabaseWriteContext.Charges.Count(c => c.OwnerId == _marketParticipantId);

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .WithMarketParticipantId(_marketParticipantId.ToString())
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByValidityNow_ReturnsChargesThatAreValidNow()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = GetPlannedCharge();
            var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime().ToInstant();

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = chargesDatabaseWriteContext.Charges.Count(
                c => c.Periods.Any(p => p.StartDateTime <= todayAtMidnightUtc));

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new SearchCriteriaDtoBuilder()
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
            actual.Should().NotContain(c => c.ChargeId == charge.SenderProvidedChargeId);
        }

        private static Charge GetPlannedCharge()
        {
            return new ChargeBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithStartDate(InstantHelper.GetTodayPlusDaysAtMidnightUtc(10))
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..5]}")
                .WithOwnerId(_marketParticipantId)
                .Build();
        }

        private static ChargesQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new ChargesQueryService(data);
            return sut;
        }

        private static Charge GetValidCharge(Domain.Charges.ChargeType chargeType = Domain.Charges.ChargeType.Subscription)
        {
            return new ChargeBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithType(chargeType)
                .WithStartDate(InstantHelper.GetTodayAtMidnightUtc())
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..10]}")
                .WithOwnerId(_marketParticipantId)
                .Build();
        }

        private static Charge GetValidChargeWithMultiplePeriods(string currentName)
        {
            var finishedPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(-7))
                .Build();
            var currentPeriod = new ChargePeriodBuilder()
                .WithName(currentName)
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(-1))
                .Build();
            var plannedPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(1))
                .Build();

            var periods = new List<ChargePeriod> { finishedPeriod, currentPeriod, plannedPeriod };

            return new ChargeBuilder()
                .WithName(Guid.NewGuid().ToString())
                .WithStartDate(InstantHelper.GetTodayAtMidnightUtc())
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..5]}")
                .WithOwnerId(_marketParticipantId)
                .AddPeriods(periods)
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

        private static IQueryable<QueryApi.Model.Charge> GetActiveCharges(QueryDbContext chargesDatabaseQueryContext)
        {
            var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();
            var expected =
                chargesDatabaseQueryContext.Charges.Where(c =>
                    c.ChargePeriods.Any(cp => cp.StartDateTime <= todayAtMidnightUtc));
            return expected;
        }
    }
}
