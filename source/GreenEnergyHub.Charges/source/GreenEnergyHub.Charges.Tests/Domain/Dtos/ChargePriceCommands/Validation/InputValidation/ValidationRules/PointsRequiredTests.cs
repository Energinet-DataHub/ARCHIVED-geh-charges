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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class PointsRequiredTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenCalledWithoutPoints_Should_ReturnFalse(
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange
            var chargePriceOperationDto = chargePriceOperationDtoBuilder.Build();
            chargePriceOperationDto.Points.Clear();
            var sut = new PointsRequiredRule(chargePriceOperationDto);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().Be(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenCalledWithPoints_Should_ReturnTrue(
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange
            var chargePriceOperationDto = chargePriceOperationDtoBuilder.Build();
            var sut = new PointsRequiredRule(chargePriceOperationDto);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().Be(true);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBy_EqualTo(
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange
            var chargePriceOperationDto = chargePriceOperationDtoBuilder.Build();

            // Act
            var sut = new PointsRequiredRule(chargePriceOperationDto);

            // Assert
            sut.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.PointsRequired);
        }
    }
}
