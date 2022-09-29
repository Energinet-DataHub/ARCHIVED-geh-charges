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

using System;
using Energinet.Charges.Contracts.Charge;
using FluentAssertions;
using GreenEnergyHub.Charges.WebApi.Mappers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.Mapper
{
    [UnitTest]
    public class ValidityOptionMapperTests
    {
        [Fact]
        public void Map_AllValidityOptions_ReturnsValidityOption()
        {
            foreach (var validityOption in Enum.GetValues<ValidityOptions>())
            {
                // Arrange
                var value = validityOption.ToString();

                // Act
                var actual = ValidityOptionMapper.Map(value);

                // Assert
                actual.Should().Be(validityOption);
            }
        }

        [Fact]
        public void Map_InvalidValidityOption_Returns()
        {
            // Arrange
            var value = "invalid_option";

            // Act
            var actual = ValidityOptionMapper.Map(value);

            // Assert
            actual.Should().Be(ValidityOptions.Now);
        }
    }
}
