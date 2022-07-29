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

using GreenEnergyHub.Charges.Domain.AvailableData.Shared;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketActivityRecord;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.MarketActivityRecord
{
    [UnitTest]
    public class ReasonCodeMapperTests
    {
        [Theory]
        [InlineAutoMoqData(ReasonCode.D01, "D01")]
        [InlineAutoMoqData(ReasonCode.D02, "D02")]
        [InlineAutoMoqData(ReasonCode.D03, "D03")]
        [InlineAutoMoqData(ReasonCode.D04, "D04")]
        [InlineAutoMoqData(ReasonCode.D05, "D05")]
        [InlineAutoMoqData(ReasonCode.D06, "D06")]
        [InlineAutoMoqData(ReasonCode.D07, "D07")]
        [InlineAutoMoqData(ReasonCode.D08, "D08")]
        [InlineAutoMoqData(ReasonCode.D09, "D09")]
        [InlineAutoMoqData(ReasonCode.D11, "D11")]
        [InlineAutoMoqData(ReasonCode.D12, "D12")]
        [InlineAutoMoqData(ReasonCode.D13, "D13")]
        [InlineAutoMoqData(ReasonCode.D14, "D14")]
        [InlineAutoMoqData(ReasonCode.D15, "D15")]
        [InlineAutoMoqData(ReasonCode.D16, "D16")]
        [InlineAutoMoqData(ReasonCode.D17, "D17")]
        [InlineAutoMoqData(ReasonCode.D18, "D18")]
        [InlineAutoMoqData(ReasonCode.D19, "D19")]
        [InlineAutoMoqData(ReasonCode.D20, "D20")]
        [InlineAutoMoqData(ReasonCode.D21, "D21")]
        [InlineAutoMoqData(ReasonCode.D22, "D22")]
        [InlineAutoMoqData(ReasonCode.D23, "D23")]
        [InlineAutoMoqData(ReasonCode.D24, "D24")]
        [InlineAutoMoqData(ReasonCode.D25, "D25")]
        [InlineAutoMoqData(ReasonCode.D26, "D26")]
        [InlineAutoMoqData(ReasonCode.D27, "D27")]
        [InlineAutoMoqData(ReasonCode.D28, "D28")]
        [InlineAutoMoqData(ReasonCode.D29, "D29")]
        [InlineAutoMoqData(ReasonCode.D30, "D30")]
        [InlineAutoMoqData(ReasonCode.D31, "D31")]
        [InlineAutoMoqData(ReasonCode.D32, "D32")]
        [InlineAutoMoqData(ReasonCode.D33, "D33")]
        [InlineAutoMoqData(ReasonCode.D34, "D34")]
        [InlineAutoMoqData(ReasonCode.D35, "D35")]
        [InlineAutoMoqData(ReasonCode.D36, "D36")]
        [InlineAutoMoqData(ReasonCode.D37, "D37")]
        [InlineAutoMoqData(ReasonCode.D38, "D38")]
        [InlineAutoMoqData(ReasonCode.D39, "D39")]
        [InlineAutoMoqData(ReasonCode.D40, "D40")]
        [InlineAutoMoqData(ReasonCode.D41, "D41")]
        [InlineAutoMoqData(ReasonCode.D42, "D42")]
        [InlineAutoMoqData(ReasonCode.D43, "D43")]
        [InlineAutoMoqData(ReasonCode.D44, "D44")]
        [InlineAutoMoqData(ReasonCode.D45, "D45")]
        [InlineAutoMoqData(ReasonCode.D46, "D46")]
        [InlineAutoMoqData(ReasonCode.D47, "D47")]
        [InlineAutoMoqData(ReasonCode.D48, "D48")]
        [InlineAutoMoqData(ReasonCode.D49, "D49")]
        [InlineAutoMoqData(ReasonCode.D50, "D50")]
        [InlineAutoMoqData(ReasonCode.D51, "D51")]
        [InlineAutoMoqData(ReasonCode.D52, "D52")]
        [InlineAutoMoqData(ReasonCode.D53, "D53")]
        [InlineAutoMoqData(ReasonCode.D54, "D54")]
        [InlineAutoMoqData(ReasonCode.D55, "D55")]
        [InlineAutoMoqData(ReasonCode.D56, "D56")]
        [InlineAutoMoqData(ReasonCode.D57, "D57")]
        [InlineAutoMoqData(ReasonCode.D58, "D58")]
        [InlineAutoMoqData(ReasonCode.D59, "D59")]
        [InlineAutoMoqData(ReasonCode.D60, "D60")]
        [InlineAutoMoqData(ReasonCode.D61, "D61")]
        [InlineAutoMoqData(ReasonCode.D62, "D62")]
        [InlineAutoMoqData(ReasonCode.D63, "D63")]
        [InlineAutoMoqData(ReasonCode.E09, "E09")]
        [InlineAutoMoqData(ReasonCode.E0H, "E0H")]
        [InlineAutoMoqData(ReasonCode.E0I, "E0I")]
        [InlineAutoMoqData(ReasonCode.E10, "E10")]
        [InlineAutoMoqData(ReasonCode.E11, "E11")]
        [InlineAutoMoqData(ReasonCode.E14, "E14")]
        [InlineAutoMoqData(ReasonCode.E16, "E16")]
        [InlineAutoMoqData(ReasonCode.E17, "E17")]
        [InlineAutoMoqData(ReasonCode.E18, "E18")]
        [InlineAutoMoqData(ReasonCode.E19, "E19")]
        [InlineAutoMoqData(ReasonCode.E22, "E22")]
        [InlineAutoMoqData(ReasonCode.E29, "E29")]
        [InlineAutoMoqData(ReasonCode.E47, "E47")]
        [InlineAutoMoqData(ReasonCode.E50, "E50")]
        [InlineAutoMoqData(ReasonCode.E51, "E51")]
        [InlineAutoMoqData(ReasonCode.E55, "E55")]
        [InlineAutoMoqData(ReasonCode.E59, "E59")]
        [InlineAutoMoqData(ReasonCode.E61, "E61")]
        [InlineAutoMoqData(ReasonCode.E73, "E73")]
        [InlineAutoMoqData(ReasonCode.E81, "E81")]
        [InlineAutoMoqData(ReasonCode.E86, "E86")]
        [InlineAutoMoqData(ReasonCode.E87, "E87")]
        [InlineAutoMoqData(ReasonCode.E90, "E90")]
        [InlineAutoMoqData(ReasonCode.E91, "E91")]
        [InlineAutoMoqData(ReasonCode.E97, "E97")]
        [InlineAutoMoqData(ReasonCode.E98, "E98")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(ReasonCode reasonCode, string expected)
        {
            var actual = ReasonCodeMapper.Map(reasonCode);
            Assert.Equal(expected, actual);
        }
    }
}
