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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class MarketParticipantQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public MarketParticipantQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync(MarketParticipantBuilder marketParticipantBuilder)
        {
            // Arrange
            var marketParticipant = marketParticipantBuilder.Build();
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            chargesDatabaseWriteContext.MarketParticipants.Add(marketParticipant);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            var expected = chargesDatabaseWriteContext.MarketParticipants.Count();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new MarketParticipantQueryService(data);

            // Act
            var actual = await sut.GetAsync();

            // Assert
            actual.Count.Should().Be(expected);
            actual.Should().Contain(m => m.Id == marketParticipant.Id);
        }
    }
}
