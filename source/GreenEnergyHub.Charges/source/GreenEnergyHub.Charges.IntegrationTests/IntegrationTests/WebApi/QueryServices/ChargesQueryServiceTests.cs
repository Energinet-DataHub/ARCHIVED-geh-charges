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
using Energinet.DataHub.Charges.Contracts.Charge;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.Data;
using GreenEnergyHub.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargesQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargesQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task SearchAsync_WhenNoSearchCriteria_ReturnsAllChargesAndPeriods()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder().Build();

            var expectedChargeV1Dtos = chargesDatabaseQueryContext.ChargePeriods.Count();
            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expectedChargeV1Dtos);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeId_ReturnsChargeV1DtoPerChargePeriodOfMatchingCharge()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeIdOrName(TestData.Charge.TestTar001.SenderProvidedChargeId)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(TestData.Charge.TestTar001.NoOfPeriods);
            actual.All(c => c.ChargeId == TestData.Charge.TestTar001.SenderProvidedChargeId).Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Subscription, ChargeType.D01)]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Fee, ChargeType.D02)]
        [InlineAutoDomainData(Domain.Charges.ChargeType.Tariff, ChargeType.D03)]
        public async Task SearchAsync_WhenSearchingByChargeType_ReturnsChargeV1DtosPerChargePeriodOfMatchingChargeType(
            Domain.Charges.ChargeType chargeType,
            ChargeType expectedChargeType)
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = GetExpectedChargeV1DtosCountPerChargeType(chargeType, chargesDatabaseQueryContext);

            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeType(expectedChargeType)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.All(c => c.ChargeType == expectedChargeType).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_WhenChargeTypeIsNull_ReturnsAllChargeTypes()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = chargesDatabaseQueryContext.ChargePeriods.Count();

            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeTypes(null!)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByMultipleChargeTypes_ReturnsChargeV1DtosPerChargePeriodOfMatchingChargeTypes()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = GetExpectedChargeV1DtosCountPerChargeType(Domain.Charges.ChargeType.Subscription, chargesDatabaseQueryContext);
            expected += GetExpectedChargeV1DtosCountPerChargeType(Domain.Charges.ChargeType.Fee, chargesDatabaseQueryContext);

            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
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
        public async Task SearchAsync_WhenSearchingByChargeName_ReturnsChargeV1DtosPerChargePeriodOfMatchingCharge()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeIdOrName(TestData.Charge.TestTar001.Name)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(TestData.Charge.TestTar001.NoOfPeriods);
            actual.All(c => c.ChargeName == TestData.Charge.TestTar001.Name).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByChargeNameButOnlyPartOfIt_ReturnsExpectedChargeV1Dtos()
        {
            // Arrange
            var searchName = TestData.Charge.TestTar001.Name[12..20];
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeIdOrName(searchName)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(TestData.Charge.TestTar001.NoOfPeriods);
            actual.All(c => c.ChargeName == TestData.Charge.TestTar001.Name).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByOwnerId_ReturnsExpectedChargeV1Dtos()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = chargesDatabaseQueryContext.ChargePeriods.Count(c =>
                c.Charge.OwnerId == SeededData.MarketParticipants.Provider8100000000030.Id);

            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithOwnerId(SeededData.MarketParticipants.Provider8100000000030.Id)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenSearchingByTwoOwnerIds_ReturnsExpectedChargeV1Dtos()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = chargesDatabaseQueryContext.ChargePeriods.Count(c =>
                c.Charge.OwnerId == SeededData.MarketParticipants.SystemOperator.Id ||
                c.Charge.OwnerId == SeededData.MarketParticipants.Provider8100000000030.Id);

            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithOwnerIds(new List<Guid>
                {
                    SeededData.MarketParticipants.SystemOperator.Id,
                    SeededData.MarketParticipants.Provider8100000000030.Id,
                })
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.All(c => c.ChargeOwner is SeededData.MarketParticipants.SystemOperator.Gln
                or SeededData.MarketParticipants.Provider8100000000030.Gln).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_WhenOwnerIdsIsNull_ReturnsAllChargesAndPeriods()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = chargesDatabaseQueryContext.ChargePeriods.Count();
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithOwnerIds(null!)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
        }

        [Fact]
        public async Task SearchAsync_WhenChargeHasMultiplePeriods_ReturnsSortedChargeV1Dtos()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var expected = chargesDatabaseQueryContext.ChargePeriods
                .Count(c => c.Charge.SenderProvidedChargeId == TestData.Charge.TestTar001.SenderProvidedChargeId);
            var searchCriteria = new ChargeSearchCriteriaV1DtoBuilder()
                .WithChargeIdOrName(TestData.Charge.TestTar001.SenderProvidedChargeId)
                .Build();

            var sut = GetSut(chargesDatabaseQueryContext);

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.Count.Should().Be(expected);
            actual.Count.Should().NotBe(0);
            actual.Should().BeInAscendingOrder(c => c.ChargeId).And
                .ThenBeInDescendingOrder(c => c.ValidFromDateTime);
        }

        private static ChargesQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new ChargesQueryService(data);
            return sut;
        }

        private static int GetExpectedChargeV1DtosCountPerChargeType(
            Domain.Charges.ChargeType chargeType,
            QueryDbContext chargesDatabaseQueryContext)
        {
            var expectedCharges = chargesDatabaseQueryContext.Charges
                .Where(c => c.Type == (int)chargeType)
                .Include(c => c.ChargePeriods)
                .ToList();

            var count = expectedCharges.Sum(charge => charge.ChargePeriods.Count);
            return count;
        }
    }
}
