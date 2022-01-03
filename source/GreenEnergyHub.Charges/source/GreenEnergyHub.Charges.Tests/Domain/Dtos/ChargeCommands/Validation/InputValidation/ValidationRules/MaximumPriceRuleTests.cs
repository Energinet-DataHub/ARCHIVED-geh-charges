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

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class MaximumPriceRuleTests
    {
        private const decimal LargestValidPrice = 999999;
        private const decimal SmallestInvalidPrice = 1000000;

        [Theory]
        [InlineAutoMoqData(999999, true)]
        [InlineAutoMoqData(999999.999999, true)]
        [InlineAutoMoqData(1000000, false)]
        [InlineAutoMoqData(1000000.000001, false)]
        public void MaximumPriceRule_WhenCalledPriceIsTooHigh_IsFalse(
            decimal price,
            bool expected,
            ChargeCommandBuilder builder)
        {
            var chargeCommand = CreateCommand(builder, price);
            var sut = new MaximumPriceRule(chargeCommand);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationError_WhenIsValid_IsNull(ChargeCommand command)
        {
            var sut = new MaximumPriceRule(command);
            sut.ValidationError.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder chargeCommandBuilder)
        {
            var command = CreateCommand(chargeCommandBuilder, SmallestInvalidPrice);
            var sut = new MaximumPriceRule(command);
            sut.ValidationError!.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.MaximumPrice);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, SmallestInvalidPrice);

            // Act
            var sut = new MaximumPriceRule(command);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargePointPrice);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargePointPosition);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = chargeCommandBuilder.WithPoint(LargestValidPrice).WithPoint(SmallestInvalidPrice).Build();
            var expectedPosition = command.ChargeOperation.Points.First(x => x.Price == SmallestInvalidPrice);

            // Act
            var sut = new MaximumPriceRule(command);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargePointPrice)
                .ParameterValue.Should().Be(expectedPosition.Price.ToString("0.##"));
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargePointPosition)
                .ParameterValue.Should().Be(expectedPosition.Position.ToString());
        }

        private static ChargeCommand CreateCommand(ChargeCommandBuilder builder, decimal price)
        {
            return builder.WithPoint(price).Build();
        }
    }
}
