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
    public class ChargeDescriptionHasMaximumLengthRuleTests
    {
        private const int ChargeDescriptionMaximumLength = 2048;

        [Theory]
        [InlineAutoMoqData(ChargeDescriptionMaximumLength - 1, true)]
        [InlineAutoMoqData(ChargeDescriptionMaximumLength, true)]
        [InlineAutoMoqData(ChargeDescriptionMaximumLength + 1, false)]
        public void ChargeDescriptionHasMaximumLengthRule_WhenDescriptionTooLong_IsFalse(
            int chargeDescriptionLength,
            bool expected,
            ChargeCommandBuilder builder)
        {
            var command = builder.WithDescription(GenerateStringWithLength(chargeDescriptionLength)).Build();
            var sut = new ChargeDescriptionHasMaximumLengthRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationError_WhenIsValid_IsNull(ChargeCommand command)
        {
            var sut = new ChargeDescriptionHasMaximumLengthRule(command);
            sut.ValidationError.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder)
        {
            var invalidCommand = CreateInvalidCommand(builder);
            var sut = new ChargeDescriptionHasMaximumLengthRule(invalidCommand);
            sut.ValidationError!.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommandBuilder builder)
        {
            // Arrange
            var invalidCommand = CreateInvalidCommand(builder);

            // Act
            var sut = new ChargeDescriptionHasMaximumLengthRule(invalidCommand);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeDescription);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(ChargeCommandBuilder builder)
        {
            // Arrange
            var invalidCommand = CreateInvalidCommand(builder);

            // Act
            var sut = new ChargeDescriptionHasMaximumLengthRule(invalidCommand);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeDescription)
                .ParameterValue.Should().Be(invalidCommand.ChargeOperation.ChargeDescription);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .ParameterValue.Should().Be(invalidCommand.ChargeOperation.ChargeId);
        }

        private static string GenerateStringWithLength(int stringLength)
        {
            return new string('a', stringLength);
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder)
        {
            var toLongDescription = new string('x', ChargeDescriptionMaximumLength + 1);
            return builder.WithDescription(toLongDescription).Build();
        }
    }
}
