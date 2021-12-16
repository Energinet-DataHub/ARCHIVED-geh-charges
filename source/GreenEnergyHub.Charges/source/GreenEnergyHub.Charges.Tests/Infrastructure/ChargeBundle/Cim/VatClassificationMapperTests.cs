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

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeBundle.Cim
{
    [UnitTest]
    public class VatClassificationMapperTests
    {
        [Theory]
        [InlineData("D01", VatClassification.NoVat)]
        [InlineData("D02", VatClassification.Vat25)]
        [InlineData("", VatClassification.Unknown)]
        [InlineData("DoesNotExist", VatClassification.Unknown)]
        [InlineData(null, VatClassification.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string vatClassification, VatClassification expected)
        {
            var actual = VatClassificationMapper.Map(vatClassification);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(VatClassification.NoVat, "D01")]
        [InlineData(VatClassification.Vat25, "D02")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(VatClassification vatClassification, string expected)
        {
            var actual = VatClassificationMapper.Map(vatClassification);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(VatClassification.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(VatClassification vatClassification)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => VatClassificationMapper.Map(vatClassification));
        }
    }
}
