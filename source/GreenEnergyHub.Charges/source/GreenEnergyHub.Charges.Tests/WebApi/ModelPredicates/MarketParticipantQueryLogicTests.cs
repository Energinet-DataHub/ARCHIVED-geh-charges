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

using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.ModelPredicates
{
    [UnitTest]
    public class MarketParticipantQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsMarketParticipantV1Dto_SetsAllProperties(MarketParticipant marketParticipant)
        {
            // Arrange
            var marketParticipants = new List<MarketParticipant> { marketParticipant }.AsQueryable();

            var expected = new MarketParticipantV1Dto(
                marketParticipant.Id,
                marketParticipant.Name,
                marketParticipant.MarketParticipantId,
                marketParticipant.BusinessProcessRole);

            // Act
            var actual = marketParticipants.AsMarketParticipantV1Dto();

            // Assert
            actual.Single().Should().BeEquivalentTo(expected);
        }
    }
}
