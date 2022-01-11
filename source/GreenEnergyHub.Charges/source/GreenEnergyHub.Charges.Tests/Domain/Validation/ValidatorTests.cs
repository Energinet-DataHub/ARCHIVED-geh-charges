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

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Validation
{
    [UnitTest]
    public class ValidatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationFails_ReturnsInvalid(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            Validator<ChargeCommand> sut,
            ChargeCommand anyCommand)
        {
            ConfigureValidatorToReturnInvalidResult(inputValidator, anyCommand);
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationSucceedsAndBusinessValidationFails_ReturnsInvalid(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            Validator<ChargeCommand> sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            ConfigureValidatorToReturnValidResult(inputValidator, anyCommand);
            ConfigureValidatorToReturnInvalidResult(businessValidator, anyCommand);

            // Act
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);

            // Assert
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationSucceedsAndBusinessValidationSucceeds_ReturnsValid(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            Validator<ChargeCommand> sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            ConfigureValidatorToReturnValidResult(inputValidator, anyCommand);
            ConfigureValidatorToReturnValidResult(businessValidator, anyCommand);

            // Act
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);

            // Assert
            Assert.False(actual.IsFailed);
        }

        private static void ConfigureValidatorToReturnValidResult(Mock<IInputValidator<ChargeCommand>> inputValidator, ChargeCommand anyCommand)
        {
            var validResult = ValidationResult.CreateSuccess();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(validResult);
        }

        private static void ConfigureValidatorToReturnInvalidResult(Mock<IInputValidator<ChargeCommand>> inputValidator, ChargeCommand anyCommand)
        {
            var invalidResult = CreateInvalidValidationResult();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(invalidResult);
        }

        private static void ConfigureValidatorToReturnValidResult(Mock<IBusinessValidator<ChargeCommand>> businessValidator, ChargeCommand anyCommand)
        {
            var validResult = ValidationResult.CreateSuccess();
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(validResult));
        }

        private static void ConfigureValidatorToReturnInvalidResult(Mock<IBusinessValidator<ChargeCommand>> businessValidator, ChargeCommand anyCommand)
        {
            var invalidResult = CreateInvalidValidationResult();
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(invalidResult));
        }

        private static ValidationResult CreateInvalidValidationResult()
        {
            var invalidRules = new List<IValidationRule> { new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation) };
            return ValidationResult.CreateFailure(invalidRules);
        }
    }
}
