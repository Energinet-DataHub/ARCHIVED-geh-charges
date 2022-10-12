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
using Energinet.Charges.Contracts.ChargePoint;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargePointQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private const string MarketParticipantOwnerId = "MarketParticipantId";
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargePointQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsPricesBasedOnSearchCriteria()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expectedPoints = new List<Point> { new(1.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0)) };
            var expectedCharge = await GetValidCharge(chargesDatabaseWriteContext, expectedPoints, Resolution.PT15M);

            var unexpectedPoints = new List<Point> { new(100.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0)) };
            var unexpectedCharge = await GetValidCharge(chargesDatabaseWriteContext, unexpectedPoints, Resolution.PT15M);

            chargesDatabaseWriteContext.Charges.Add(unexpectedCharge);
            chargesDatabaseWriteContext.Charges.Add(expectedCharge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                expectedCharge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            var expected = new ChargePointV1Dto(
                expectedPoints.Single().Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(expectedPoints.Count);
            actual.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsPricesInsideSearchDateInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expectedPoint = new Point(1.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0));
            var unexpectedPoint = new Point(10.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(2));
            var expectedPoints = new List<Point> { expectedPoint, unexpectedPoint };

            var expectedCharge = await GetValidCharge(chargesDatabaseWriteContext, expectedPoints, Resolution.PT15M);

            chargesDatabaseWriteContext.Charges.Add(expectedCharge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                expectedCharge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            var expected = new ChargePointV1Dto(
                expectedPoint.Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(1);
            actual.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsPricesWith15MinutesInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15)),
                new(3.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(30)),
                new(4.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(45)),
                new(5.00m, InstantHelper.GetTodayPlusMinutesAtMidnightUtc(60)),
            };

            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.PT15M);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a => (a.ActiveToDateTime - a.ActiveFromDateTime).Minutes == 15));
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsAllPricesWithOneHourInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1)),
                new(3.00m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(2)),
                new(4.00m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(3)),
                new(5.00m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(4)),
            };

            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.PT1H);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a => (a.ActiveToDateTime - a.ActiveFromDateTime).Hours == 1));
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsAllPricesWithOneDayInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
                new(3.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(2)),
                new(4.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(3)),
                new(5.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(4)),
            };

            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(0).ToDateTimeUtc(),
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(4).ToDateTimeUtc());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a => (a.ActiveToDateTime - a.ActiveFromDateTime).Days == 1));
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsAllPricesWithOneMonthInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(1)),
                new(3.00m, InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(2)),
                new(4.00m, InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(3)),
                new(5.00m, InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(4)),
            };

            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1M);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new SearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(0).ToDateTimeUtc(),
                InstantHelper.GetThisMonthPlusMonthsAtMidnightUtc(4).ToDateTimeUtc());

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a =>
                a.ActiveFromDateTime.Month == a.ActiveToDateTime.Month &&
                a.ActiveFromDateTime.Day == 1 &&
                DateTime.DaysInMonth(a.ActiveFromDateTime.Year, a.ActiveFromDateTime.Month) == a.ActiveToDateTime.Day));
        }

        private static async Task<Charge> GetValidCharge(
            ChargesDatabaseContext chargesDatabaseWriteContext,
            List<Point> points,
            Resolution resolution)
        {
            var marketParticipantId =
                await GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext, MarketParticipantOwnerId);

            var charge = new ChargeBuilder()
                .WithStartDate(InstantHelper.GetTodayAtMidnightUtc())
                .WithOwnerId(marketParticipantId)
                .WithMarketParticipantRole(MarketParticipantRole.SystemOperator)
                .WithResolution(resolution)
                .WithPoints(points)
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..10]}")
                .Build();
            return charge;
        }

        private static ChargePointQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var configuration = new Iso8601ConversionConfiguration("Europe/Copenhagen");
            var iso8601Durations = new Iso8601Durations(configuration);
            var sut = new ChargePointQueryService(data, iso8601Durations);
            return sut;
        }

        private static async Task<Guid> GetOrAddMarketParticipantAsync(
            ChargesDatabaseContext context,
            string marketParticipantOwnerId)
        {
            var marketParticipant = await context
                .MarketParticipants
                .SingleOrDefaultAsync(x => x.MarketParticipantId == marketParticipantOwnerId);

            if (marketParticipant != null)
            {
                return marketParticipant.Id;
            }

            marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                marketParticipantOwnerId,
                MarketParticipantStatus.Active,
                MarketParticipantRole.GridAccessProvider);
            context.MarketParticipants.Add(marketParticipant);
            await context.SaveChangesAsync();

            return marketParticipant.Id;
        }
    }
}
