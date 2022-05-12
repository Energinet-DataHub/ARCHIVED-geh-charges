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
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.Charges
{
    [UnitTest]
    public class TransparentInvoicingMapperTests
    {
        [Theory]
        [InlineData(false, TransparentInvoicing.NonTransparent)]
        [InlineData(true, TransparentInvoicing.Transparent)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(bool transparentInvoicing, TransparentInvoicing expected)
        {
            var actual = TransparentInvoicingMapper.Map(transparentInvoicing);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(TransparentInvoicing.NonTransparent, false)]
        [InlineData(TransparentInvoicing.Transparent, true)]
        public void Map_WhenGivenKnownInput_MapsToCorrectBoolean(TransparentInvoicing transparentInvoicing, bool expected)
        {
            var actual = TransparentInvoicingMapper.Map(transparentInvoicing);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(TransparentInvoicing.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(TransparentInvoicing transparentInvoicing)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => TransparentInvoicingMapper.Map(transparentInvoicing));
        }
    }
}
