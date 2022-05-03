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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.Charges
{
    [UnitTest]
    public class TaxIndicatorMapperTests
    {
        [Theory]
        [InlineData(false, TaxIndicator.NoTax)]
        [InlineData(true, TaxIndicator.Tax)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(bool taxIndicator, TaxIndicator expected)
        {
            var actual = TaxIndicatorMapper.Map(taxIndicator);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(TaxIndicator.NoTax, false)]
        [InlineData(TaxIndicator.Tax, true)]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(TaxIndicator taxIndicator, bool expected)
        {
            var actual = TaxIndicatorMapper.Map(taxIndicator);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(TaxIndicator.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(TaxIndicator taxIndicator)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => TaxIndicatorMapper.Map(taxIndicator));
        }
    }
}
