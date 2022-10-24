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
using GreenEnergyHub.Charges.TestCore.Builders.Query;
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(expectedCharge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            var expected = new ChargePriceV1Dto(
                expectedPoints.Single().Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(expectedPoints.Count);
            actual.ChargePrices.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingHasSkip_ReturnsPrices()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var pointToSkip = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var pointToSkip2 = new Point(50m, InstantHelper.GetTodayAtMidnightUtc());
            var expectedPoint = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { pointToSkip, pointToSkip2, expectedPoint };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithSkip(2)
                .Build();

            var expected = new ChargePriceV1Dto(
                expectedPoint.Price,
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(1).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(1);
            actual.ChargePrices.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingHasNoSkip_ReturnsPrices()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expectedPoint = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var point1 = new Point(50m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point2 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point1, point2, expectedPoint };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithTake(1)
                .Build();

            var expected = new ChargePriceV1Dto(
                expectedPoint.Price,
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset(),
                InstantHelper.GetTodayPlusDaysAtMidnightUtc(1).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(1);
            actual.ChargePrices.Single().Should().Be(expected);
        }

        [Fact]
        public async Task SearchAsync_WhenSearching_ReturnsTotalAmountOfPrices()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(50m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point1, point2, point3 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithTake(1)
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(1);
            actual.TotalAmount.Should().Be(points.Count);
        }

        [Fact]
        public async Task SearchAsync_WhenSortColumnNameIsNotValid_ReturnsPricesSortedByFromDateTime()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(50m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));

            var points = new List<Point> { point1, point2, point3 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithSortColumnName((SortColumnName)(-1))
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            actual.ChargePrices.Should().BeInAscendingOrder(cp => cp.FromDateTime);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingIsDescendingOrderOnFromDateTime_ReturnsPricesInDescendingOrder()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(50m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point2, point3,  point1 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithIsDescending(true)
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            actual.ChargePrices.Should().BeInDescendingOrder(cp => cp.FromDateTime);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingIsAscendingOrderOnFromDateTime_ReturnsPricesInAscendingOrder()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(10m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(50m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point2, point3, point1 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithIsDescending(false)
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            actual.ChargePrices.Should().BeInAscendingOrder(cp => cp.FromDateTime);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingIsAscendingOrderOnPrice_ReturnsPricesInAscendingOrder()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(100m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(200m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point2, point3, point1 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithSortColumnName(SortColumnName.Price)
                .WithIsDescending(false)
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            actual.ChargePrices.Should().BeInAscendingOrder(cp => cp.Price);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingIsDescendingOrderOnPrice_ReturnsPricesInDescendingOrder()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var point1 = new Point(100m, InstantHelper.GetTodayAtMidnightUtc());
            var point2 = new Point(200m, InstantHelper.GetTodayPlusHoursAtMidnightUtc(1));
            var point3 = new Point(20m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1));

            var points = new List<Point> { point2, point3, point1 };
            var charge = await GetValidCharge(chargesDatabaseWriteContext, points, Resolution.P1D);

            chargesDatabaseWriteContext.Charges.Add(charge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2).ToDateTimeOffset())
                .WithSortColumnName(SortColumnName.Price)
                .WithIsDescending(true)
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            actual.ChargePrices.Should().BeInDescendingOrder(cp => cp.Price);
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(expectedCharge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            var expected = new ChargePriceV1Dto(
                expectedPoint.Price,
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(0).ToDateTimeOffset(),
                InstantHelper.GetTodayPlusMinutesAtMidnightUtc(15).ToDateTimeOffset());

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(1);
            actual.ChargePrices.Single().Should().Be(expected);
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            foreach (var chargePrice in actual.ChargePrices)
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            foreach (var chargePrice in actual.ChargePrices)
                (chargePrice.ToDateTime - chargePrice.FromDateTime).Hours.Should().Be(1);
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(5).ToDateTimeOffset())
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            foreach (var chargePrice in actual.ChargePrices)
                (chargePrice.ToDateTime - chargePrice.FromDateTime).Days.Should().Be(1);
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
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder()
                .WithChargeId(charge.Id)
                .WithFromDateTime(InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(0).ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetFirstDayOfThisMonthPlusMonthsAtMidnightUtc(5).ToDateTimeOffset())
                .Build();

            // Act
            var actual = sut.Search(searchCriteria);

            // Assert
            actual.ChargePrices.Should().HaveCount(points.Count);
            foreach (var chargePrice in actual.ChargePrices)
            {
                chargePrice.FromDateTime.Month.Should().NotBe(chargePrice.ToDateTime.Month);
                DateTime.DaysInMonth(chargePrice.FromDateTime.DayOfYear, chargePrice.FromDateTime.Month).Should()
                    .Be(chargePrice.FromDateTime.Day);
                DateTime.DaysInMonth(chargePrice.ToDateTime.Year, chargePrice.ToDateTime.Month).Should()
                    .Be(chargePrice.ToDateTime.Day);
            }
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
