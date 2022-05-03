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
    public class BusinessReasonCodeMapperTests
    {
        [Theory]
        [InlineData("D08", BusinessReasonCode.UpdateChargePrices)]
        [InlineData("D17", BusinessReasonCode.UpdateMasterDataSettlement)]
        [InlineData("D18", BusinessReasonCode.UpdateChargeInformation)]
        [InlineData("", BusinessReasonCode.Unknown)]
        [InlineData("DoesNotExist", BusinessReasonCode.Unknown)]
        [InlineData(null, BusinessReasonCode.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, BusinessReasonCode expected)
        {
            var actual = BusinessReasonCodeMapper.Map(unit);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(BusinessReasonCode.UpdateChargePrices, "D08")]
        [InlineData(BusinessReasonCode.UpdateChargeInformation, "D18")]
        [InlineData(BusinessReasonCode.UpdateMasterDataSettlement, "D17")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(BusinessReasonCode businessReasonCode, string expected)
        {
            var actual = BusinessReasonCodeMapper.Map(businessReasonCode);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(BusinessReasonCode.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(BusinessReasonCode businessReasonCode)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => BusinessReasonCodeMapper.Map(businessReasonCode));
        }
    }
}
