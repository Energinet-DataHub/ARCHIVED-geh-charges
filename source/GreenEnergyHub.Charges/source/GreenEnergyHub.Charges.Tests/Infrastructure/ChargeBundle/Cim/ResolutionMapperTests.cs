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
    public class ResolutionMapperTests
    {
        [Theory]
        [InlineData("P1D", Resolution.P1D)]
        [InlineData("P1M", Resolution.P1M)]
        [InlineData("PT1H", Resolution.PT1H)]
        [InlineData("PT15M", Resolution.PT15M)]
        [InlineData("", Resolution.Unknown)]
        [InlineData("DoesNotExist", Resolution.Unknown)]
        [InlineData(null, Resolution.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string resolution, Resolution expected)
        {
            var actual = ResolutionMapper.Map(resolution);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(Resolution.P1D, "P1D")]
        [InlineData(Resolution.P1M, "P1M")]
        [InlineData(Resolution.PT1H, "PT1H")]
        [InlineData(Resolution.PT15M, "PT15M")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(Resolution resolution, string expected)
        {
            var actual = ResolutionMapper.Map(resolution);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(Resolution.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(Resolution resolution)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => ResolutionMapper.Map(resolution));
        }
    }
}
