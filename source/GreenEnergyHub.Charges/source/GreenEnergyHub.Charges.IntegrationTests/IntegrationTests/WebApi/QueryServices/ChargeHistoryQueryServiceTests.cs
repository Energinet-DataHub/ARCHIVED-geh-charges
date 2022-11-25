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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeHistory;
using FluentAssertions;
using FluentAssertions.Common;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using Microsoft.Azure.Amqp.Serialization;
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
        [InlineAutoMoqData("2022-01-01T06:14:00Z", 1, new string[] { "Name A0" })] // atm incorrect input date
        [InlineAutoMoqData("2022-01-01T07:16:00Z", 1, new string[] { "Name A1" })]
        [InlineAutoMoqData("2022-01-03T21:01:00Z", 2, new string[] { "Name A1", "Name B" })]
        [InlineAutoMoqData("2022-01-04T20:01:00Z", 2, new string[] { "Name A2", "Name B" })]
        [InlineAutoMoqData("2022-01-05T20:01:00Z", 3, new string[] { "Name A2", "Name B", "Name future" })]
        public async Task GetAsync_WhenCalled_ReturnsChargeHistoryBasedOnSearchCriteria(
            string atDateTime,
            int expectedDtos,
            string[] expectedNames)
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);

            var atDateTimeOffset = DateTime.Parse(atDateTime, CultureInfo.InvariantCulture).ToDateTimeOffset();

            var searchCriteria = new ChargeHistorySearchCriteriaV1DtoBuilder()
                .WithChargeId("TariffA")
                .WithChargeType(ChargeType.D03)
                .WithChargeOwner("8100000000030")
                .WithAtDateTime(atDateTimeOffset)
                .Build();

            var actual = await sut.GetAsync(searchCriteria);

            actual.Should().HaveCount(expectedDtos);

            var listOfNames = GetListOfNames(actual);
            listOfNames.Should().ContainInOrder(expectedNames);
        }

        private static IEnumerable<string> GetListOfNames(IEnumerable<ChargeHistoryV1Dto> actual)
        {
           return actual.Select(c => c.Name).ToArray();
        }

        private static ChargeHistoryQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new ChargeHistoryQueryService(data);
            return sut;
        }
    }
}
