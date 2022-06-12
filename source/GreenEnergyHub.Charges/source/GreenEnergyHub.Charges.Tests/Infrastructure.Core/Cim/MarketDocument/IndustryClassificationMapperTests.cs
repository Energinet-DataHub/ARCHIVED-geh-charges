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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Cim.MarketDocument
{
    [UnitTest]
    public class IndustryClassificationMapperTests
    {
        [Theory]
        [InlineData("23", IndustryClassification.Electricity)]
        [InlineData("", IndustryClassification.Unknown)]
        [InlineData("DoesNotExist", IndustryClassification.Unknown)]
        [InlineData(null, IndustryClassification.Unknown)]
        public void Map_WhenGivenInput_MapsToCorrectEnum(string unit, IndustryClassification expected)
        {
            var actual = IndustryClassificationMapper.Map(unit);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(IndustryClassification.Electricity, "23")]
        public void Map_WhenGivenKnownInput_MapsToCorrectString(IndustryClassification industryClassification, string expected)
        {
            var actual = IndustryClassificationMapper.Map(industryClassification);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(IndustryClassification.Unknown)]
        public void Map_WhenGivenUnknownInput_ThrowsExceptions(IndustryClassification industryClassification)
        {
            Assert.Throws<InvalidEnumArgumentException>(() => IndustryClassificationMapper.Map(industryClassification));
        }
    }
}
