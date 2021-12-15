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
using GreenEnergyHub.Charges.MessageHub.Infrastructure.MarketActivityRecord;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkReceiptBundle.Cim
{
    [UnitTest]
    public class ReasonCodeMapperTests
    {
        [Theory]
        [InlineData(ReasonCode.IncorrectChargeInformation, "D14")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(ReasonCode reasonCode, string expected)
        {
            var actual = ReasonCodeMapper.Map(reasonCode);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ReasonCode.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(ReasonCode reasonCode)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => ReasonCodeMapper.Map(reasonCode));
        }
    }
}
