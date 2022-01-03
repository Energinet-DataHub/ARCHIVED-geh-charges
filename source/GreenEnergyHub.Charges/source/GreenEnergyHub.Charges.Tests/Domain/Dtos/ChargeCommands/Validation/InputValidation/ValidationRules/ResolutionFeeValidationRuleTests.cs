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
using GreenEnergyHub.Charges.Domain.Charges;
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
    public class ResolutionFeeValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionFeeValidationRule_WithTariffType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Tariff, resolution);

            // Act
            var sut = new ResolutionFeeValidationRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionFeeValidationRule_WithSubscriptionType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Subscription, resolution);

            // Act
            var sut = new ResolutionFeeValidationRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, false)]
        [InlineAutoMoqData(Resolution.PT1H, false)]
        [InlineAutoMoqData(Resolution.PT15M, false)]
        public void ResolutionFeeValidationRule_WithFeeType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Fee, resolution);

            // Act
            var sut = new ResolutionFeeValidationRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationError_WhenIsValid_IsNull(ChargeCommand command)
        {
            var sut = new ResolutionFeeValidationRule(command);
            sut.ValidationError.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder chargeCommandBuilder)
        {
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Fee, Resolution.Unknown);
            var sut = new ResolutionFeeValidationRule(command);
            sut.ValidationError!.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ResolutionFeeValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Fee, Resolution.Unknown);

            // Act
            var sut = new ResolutionFeeValidationRule(command);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeResolution);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeType);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(
            ChargeCommandBuilder chargeCommandBuilder)
        {
            // Arrange
            var command = CreateCommand(chargeCommandBuilder, ChargeType.Fee, Resolution.Unknown);

            // Act
            var sut = new ResolutionFeeValidationRule(command);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeResolution)
                .ParameterValue.Should().Be(command.ChargeOperation.Resolution.ToString());
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .ParameterValue.Should().Be(command.ChargeOperation.ChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeType)
                .ParameterValue.Should().Be(command.ChargeOperation.Type.ToString());
        }

        private static ChargeCommand CreateCommand(ChargeCommandBuilder builder, ChargeType chargeType, Resolution resolution)
        {
            return builder.WithChargeType(chargeType).WithResolution(resolution).Build();
        }
    }
}
