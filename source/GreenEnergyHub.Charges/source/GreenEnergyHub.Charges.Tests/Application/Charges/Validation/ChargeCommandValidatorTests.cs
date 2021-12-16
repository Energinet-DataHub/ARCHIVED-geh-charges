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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation
{
    [UnitTest]
    public class ChargeCommandValidatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationFails_ReturnsInvalid(
            [NotNull] [Frozen] Mock<IChargeCommandInputValidator> inputValidator,
            [NotNull] ChargeCommandValidator sut,
            ChargeCommand anyCommand)
        {
            ConfigureValidatorToReturnInvalidResult(inputValidator, anyCommand);
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationSucceedsAndBusinessValidationFails_ReturnsInvalid(
            [NotNull] [Frozen] Mock<IChargeCommandInputValidator> inputValidator,
            [NotNull] [Frozen] Mock<IChargeCommandBusinessValidator> businessValidator,
            [NotNull] ChargeCommandValidator sut,
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
            [NotNull] [Frozen] Mock<IChargeCommandInputValidator> inputValidator,
            [NotNull] [Frozen] Mock<IChargeCommandBusinessValidator> businessValidator,
            [NotNull] ChargeCommandValidator sut,
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

        private static void ConfigureValidatorToReturnValidResult(Mock<IChargeCommandInputValidator> inputValidator, ChargeCommand anyCommand)
        {
            var validResult = ChargeCommandValidationResult.CreateSuccess();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(validResult);
        }

        private static void ConfigureValidatorToReturnInvalidResult(Mock<IChargeCommandInputValidator> inputValidator, ChargeCommand anyCommand)
        {
            var invalidResult = CreateInvalidValidationResult();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(invalidResult);
        }

        private static void ConfigureValidatorToReturnValidResult(Mock<IChargeCommandBusinessValidator> businessValidator, ChargeCommand anyCommand)
        {
            var validResult = ChargeCommandValidationResult.CreateSuccess();
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(validResult));
        }

        private static void ConfigureValidatorToReturnInvalidResult(Mock<IChargeCommandBusinessValidator> businessValidator, ChargeCommand anyCommand)
        {
            var invalidResult = CreateInvalidValidationResult();
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(invalidResult));
        }

        private static ChargeCommandValidationResult CreateInvalidValidationResult()
        {
            var invalidRules = new List<IValidationRule> { new ValidationRule(false, ValidationRuleIdentifier.StartDateValidation) };
            return ChargeCommandValidationResult.CreateFailure(invalidRules);
        }
    }
}
