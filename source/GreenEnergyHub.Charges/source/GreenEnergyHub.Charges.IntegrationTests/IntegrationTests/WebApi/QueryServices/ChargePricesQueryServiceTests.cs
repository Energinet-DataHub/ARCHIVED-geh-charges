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
using Energinet.DataHub.Charges.Contracts.ChargePrice;
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
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargePricesQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargePricesQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsPricesBasedOnSearchCriteria()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expectedPoints = new List<Point> { new(1.00m, InstantHelper.GetTodayAtMidnightUtc()) };
            var expectedCharge = await GetValidCharge(chargesDatabaseWriteContext, expectedPoints, Resolution.PT15M);

            var unexpectedPoints = new List<Point> { new(100.00m, InstantHelper.GetTodayAtMidnightUtc()) };
            var unexpectedCharge = await GetValidCharge(chargesDatabaseWriteContext, unexpectedPoints, Resolution.PT15M);

            chargesDatabaseWriteContext.Charges.Add(unexpectedCharge);
            chargesDatabaseWriteContext.Charges.Add(expectedCharge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                expectedCharge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            var expected = new ChargePriceV1Dto(
                expectedPoints.Single().Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.Should().HaveCount(expectedPoints.Count);
            actual.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsPricesInsideSearchDateInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expectedPoint = new Point(1.00m, InstantHelper.GetTodayAtMidnightUtc());
            var unexpectedPoint = new Point(10.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));
            var points = new List<Point> { expectedPoint, unexpectedPoint };

            var expectedCharge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.PT15M);

            chargesDatabaseWriteContext.Charges.Add(expectedCharge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                expectedCharge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            var expected = new ChargePriceV1Dto(
                expectedPoint.Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

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
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            foreach (var chargePrice in actual)
                (chargePrice.ToDateTime - chargePrice.FromDateTime).Minutes.Should().Be(15);
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
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeUtc());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a => (a.ToDateTime - a.FromDateTime).Hours == 1));
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
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(0).ToDateTimeUtc(),
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(5).ToDateTimeUtc());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a => (a.ToDateTime - a.FromDateTime).Days == 1));
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsAllPricesWithOneMonthInterval()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(1)),
                new(3.00m, InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(2)),
                new(4.00m, InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(3)),
                new(5.00m, InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(4)),
            };

            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1M);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(
                charge.Id,
                InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(0).ToDateTimeUtc(),
                InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(5).ToDateTimeUtc());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.Should().HaveCount(points.Count);
            Assert.True(actual.All(a =>
                a.FromDateTime.Month != a.ToDateTime.Month &&
                DateTime.DaysInMonth(a.FromDateTime.DayOfYear, a.FromDateTime.Month) == a.FromDateTime.Day &&
                DateTime.DaysInMonth(a.ToDateTime.Year, a.ToDateTime.Month) == a.ToDateTime.Day));
        }

        private static async Task<Charge> GetValidCharge(
            ChargesDatabaseContext chargesDatabaseWriteContext,
            List<Point> points,
            Resolution resolution)
        {
            var marketParticipantId =
                await QueryServiceAutoMoqDataFixer.GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

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

        private static ChargePriceQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var configuration = new Iso8601ConversionConfiguration("Europe/Copenhagen");
            var iso8601Durations = new Iso8601Durations(configuration);
            var sut = new ChargePriceQueryService(data, iso8601Durations);
            return sut;
        }
    }
}
