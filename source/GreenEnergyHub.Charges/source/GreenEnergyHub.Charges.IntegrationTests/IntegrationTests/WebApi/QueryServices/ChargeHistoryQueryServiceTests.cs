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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.Data;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargeHistoryQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeHistoryQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData("2022-01-01T07:14:00Z", 1, new[] { "Name A0" })]
        [InlineAutoMoqData("2022-01-01T07:16:00Z", 1, new[] { "Name A1" })]
        [InlineAutoMoqData("2022-01-03T21:01:00Z", 2, new[] { "Name A1", "Name B" })]
        [InlineAutoMoqData("2022-01-04T20:01:00Z", 2, new[] { "Name A2", "Name B" })]
        [InlineAutoMoqData("2022-01-05T20:01:00Z", 3, new[] { "Name A2", "Name B", "Name future" })]
        public async Task SearchAsync_WhenCalled_ReturnsChargeHistoryBasedOnSearchCriteria(
            string atDateTime,
            int expectedDtos,
            string[] expectedNames)
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);

            var atDateTimeOffset = DateTime.Parse(atDateTime, CultureInfo.InvariantCulture).ToUniversalTime().ToDateTimeOffset();

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(TestData.ChargeHistory.TariffA.Id)
                .WithAtDateTime(atDateTimeOffset)
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            using var assertionScope = new AssertionScope();
            actual.Should().HaveCount(expectedDtos);

            var listOfNames = actual.Select(c => c.Name).ToArray();
            listOfNames.Should().ContainInOrder(expectedNames);
        }

        [Theory]
        [InlineAutoMoqData("2022-01-05T20:01:00Z", 3)]
        public async Task SearchAsync_WhenCalled_ReturnsChargeHistoryOrderedByStartDateAscending(
            string atDateTime,
            int expectedDtos)
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);

            var atDateTimeOffset = DateTime.Parse(atDateTime, CultureInfo.InvariantCulture).ToUniversalTime().ToDateTimeOffset();

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(TestData.ChargeHistory.TariffA.Id)
                .WithAtDateTime(atDateTimeOffset)
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            using var assertionScope = new AssertionScope();
            actual.Should().HaveCount(expectedDtos);
            actual.Should().BeInAscendingOrder(x => x.StartDateTime);
        }

        [Theory]
        [InlineAutoMoqData("2022-01-05T20:01:00Z", 3)]
        public async Task SearchAsync_WhenCalled_ReturnsChargeHistoryWithCorrectPeriods(
            string atDateTime,
            int expectedDtos)
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);

            var atDateTimeOffset = DateTime.Parse(atDateTime, CultureInfo.InvariantCulture).ToUniversalTime().ToDateTimeOffset();

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(TestData.ChargeHistory.TariffA.Id)
                .WithAtDateTime(atDateTimeOffset)
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            using var assertionScope = new AssertionScope();
            actual.Should().HaveCount(expectedDtos);

            var firstHistory = actual[0];
            var secondHistory = actual[1];
            var thirdHistory = actual[2];

            firstHistory.EndDateTime.Should().Be(secondHistory.StartDateTime);
            secondHistory.EndDateTime.Should().Be(thirdHistory.StartDateTime);
            thirdHistory.EndDateTime.Should().BeNull();
        }

        [Fact]
        public async Task SearchAsync_WhenCalled_ReturnsChargeHistoryV1DtoWithCorrectValues()
        {
            // Arrange
            await using var chargesQueryDbContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesQueryDbContext);

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(TestData.ChargeHistory.HistTar001.Id)
                .WithAtDateTime(DateTimeOffset.Now)
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            var dto = actual.Single();
            using var assertionScope = new AssertionScope();

            dto.StartDateTime.Should().Be(TestData.ChargeHistory.HistTar001.StartDateTime);
            dto.EndDateTime.Should().BeNull();
            dto.Name.Should().Be(TestData.ChargeHistory.HistTar001.Name);
            dto.Description.Should().Be(TestData.ChargeHistory.HistTar001.Description);
            dto.Resolution.Should().Be(TestData.ChargeHistory.HistTar001.Resolution);
            dto.VatClassification.Should().Be(TestData.ChargeHistory.HistTar001.VatClassification);
            dto.TaxIndicator.Should().Be(TestData.ChargeHistory.HistTar001.TaxIndicator);
            dto.TransparentInvoicing.Should().Be(TestData.ChargeHistory.HistTar001.TransparentInvoicing);
            dto.ChargeType.Should().Be(TestData.ChargeHistory.HistTar001.ChargeType);
            dto.ChargeOwner.Should().Be(TestData.ChargeHistory.HistTar001.ChargeOwner);
        }

        [Fact]
        public async Task SearchAsync_WhenCalled_ReturnsChargeHistoryWithAStop()
        {
            // Arrange
            await using var chargesQueryDbContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesQueryDbContext);

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(TestData.ChargeHistory.TariffB.Id)
                .WithAtDateTime(DateTimeOffset.Now)
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            using var assertionScope = new AssertionScope();

            actual[0].StartDateTime.Should().Be(TestData.ChargeHistory.TariffB.FirstStartDateTime);
            actual[0].EndDateTime.Should().Be(TestData.ChargeHistory.TariffB.SecondStartDateTime);
            actual[1].StartDateTime.Should().Be(TestData.ChargeHistory.TariffB.SecondStartDateTime);
            actual[1].EndDateTime.Should().Be(TestData.ChargeHistory.TariffB.SecondStartDateTime);
        }

        [Fact]
        public async Task SearchAsync_WhenChargeDoesNotExist_ThrowsArgumentException()
        {
            // Arrange
            await using var chargesQueryDbContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesQueryDbContext);
            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId(Guid.NewGuid())
                .WithAtDateTime(DateTimeOffset.Now)
                .Build();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SearchAsync(searchCriteria));
        }

        private static ChargeHistoryQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new ChargeHistoryQueryService(data);
            return sut;
        }
    }
}
