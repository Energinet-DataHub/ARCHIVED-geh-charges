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
using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.Charges
{
    [UnitTest]
    public class ChargeTypeMapperTests
    {
        [Theory]
        [InlineData("D01", ChargeType.Subscription)]
        [InlineData("D02", ChargeType.Fee)]
        [InlineData("D03", ChargeType.Tariff)]
        [InlineData("", ChargeType.Unknown)]
        [InlineData("DoesNotExist", ChargeType.Unknown)]
        [InlineData(null, ChargeType.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, ChargeType expected)
        {
            var actual = ChargeTypeMapper.Map(unit);
            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData(ChargeType.Fee, "D02")]
        [InlineData(ChargeType.Subscription, "D01")]
        [InlineData(ChargeType.Tariff, "D03")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(ChargeType chargeType, string expected)
        {
            var actual = ChargeTypeMapper.Map(chargeType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ChargeType.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(ChargeType chargeType)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => ChargeTypeMapper.Map(chargeType));
        }

        [Fact]
        public void MayMany_WhenGivenKnownInput_MapsToCorrectEnums()
        {
            // Arrange
            var chargeTypes = "D01,D02,D03";
            var expected = new List<ChargeType> { ChargeType.Subscription, ChargeType.Fee, ChargeType.Tariff };

            // Act
            var actual = ChargeTypeMapper.MapMany(chargeTypes);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MayMany_WhenGivenUnknownInput_MapsToUnknown()
        {
            // Arrange
            var chargeTypes = "not_a_type";
            var expected = new List<ChargeType> { ChargeType.Unknown };

            // Act
            var actual = ChargeTypeMapper.MapMany(chargeTypes);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
