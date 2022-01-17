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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.MarketDocument
{
    [UnitTest]
    public class MarketParticipantRoleMapperTests
    {
        [Theory]
        [InlineData("DDQ", MarketParticipantRole.EnergySupplier)]
        [InlineData("DDM", MarketParticipantRole.GridAccessProvider)]
        [InlineData("DDZ", MarketParticipantRole.MeteringPointAdministrator)]
        [InlineData("EZ", MarketParticipantRole.SystemOperator)]
        [InlineData("", MarketParticipantRole.Unknown)]
        [InlineData("DoesNotExist", MarketParticipantRole.Unknown)]
        [InlineData(null, MarketParticipantRole.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, MarketParticipantRole expected)
        {
            var actual = MarketParticipantRoleMapper.Map(unit);
            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData(MarketParticipantRole.EnergySupplier, "DDQ")]
        [InlineData(MarketParticipantRole.GridAccessProvider, "DDM")]
        [InlineData(MarketParticipantRole.MeteringPointAdministrator, "DDZ")]
        [InlineData(MarketParticipantRole.SystemOperator, "EZ")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(MarketParticipantRole marketParticipantRole, string expected)
        {
            var actual = MarketParticipantRoleMapper.Map(marketParticipantRole);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MarketParticipantRole.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(MarketParticipantRole marketParticipantRole)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => MarketParticipantRoleMapper.Map(marketParticipantRole));
        }
    }
}
