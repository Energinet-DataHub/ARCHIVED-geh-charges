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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.BusinessRules.ValidationRules
{
    [UnitTest]
    public class PriceSeriesChargeDoesNotExistTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenCalledWithCharge_Should_ReturnTrue(
            ChargeBuilder chargeBuilder)
        {
            // Arrange
            var sut = new PriceSeriesChargeMustExistRule(chargeBuilder.Build());

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WhenCalledWithNull_Should_ReturnFalse()
        {
            // Arrange
            var sut = new PriceSeriesChargeMustExistRule(null!);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBePriceSeriesMustMatchChargeResolution()
        {
            // Arrange
            // Act
            var sut = new PriceSeriesChargeMustExistRule(null);

            // Assert
            sut.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.PriceSeriesChargeDoesNotExist);
        }
    }
}
