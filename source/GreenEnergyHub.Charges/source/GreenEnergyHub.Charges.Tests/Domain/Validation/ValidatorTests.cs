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
        public void ValidateAsync_WhenInputValidationFails_ReturnsInvalid(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            Validator<ChargeCommand, ChargeOperationComposite> sut,
            ChargeCommand anyCommand)
        {
            ConfigureValidatorToReturnInvalidResult(inputValidator, anyCommand);
            var actual = sut.InputValidate(anyCommand);
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenBusinessValidationFails_ReturnsInvalid(
            [Frozen] Mock<IBusinessValidator<ChargeCommand, ChargeOperationComposite>> businessValidator,
            Validator<ChargeCommand, ChargeOperationComposite> sut,
            ChargeOperationComposite anyComposite)
        {
            ConfigureValidatorToReturnInvalidResult(businessValidator, anyComposite);
            var actual = await sut.BusinessValidateAsync(anyComposite).ConfigureAwait(false);
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidateAsync_WhenInputValidationSucceeds_ReturnsValid(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            Validator<ChargeCommand, ChargeOperationComposite> sut,
            ChargeCommand anyCommand)
        {
            ConfigureValidatorToReturnValidResult(inputValidator, anyCommand);
            var actual = sut.InputValidate(anyCommand);
            Assert.False(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenBusinessValidationSucceeds_ReturnsValid(
            [Frozen] Mock<IBusinessValidator<ChargeCommand, ChargeOperationComposite>> businessValidator,
            Validator<ChargeCommand, ChargeOperationComposite> sut,
            ChargeOperationComposite anyComposite)
        {
            ConfigureValidatorToReturnValidResult(businessValidator, anyComposite);
            var actual = await sut.BusinessValidateAsync(anyComposite).ConfigureAwait(false);
            Assert.False(actual.IsFailed);
        }

        private static void ConfigureValidatorToReturnValidResult(
            Mock<IInputValidator<ChargeCommand>> inputValidator, ChargeCommand anyCommand)
        {
            var validResult = ValidationResult.CreateSuccess();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(validResult);
        }

        private static void ConfigureValidatorToReturnInvalidResult(
            Mock<IInputValidator<ChargeCommand>> inputValidator, ChargeCommand anyCommand)
        {
            var invalidResult = CreateInvalidValidationResult();
            inputValidator.Setup(v => v.Validate(anyCommand)).Returns(invalidResult);
        }

        private static void ConfigureValidatorToReturnValidResult(
            Mock<IBusinessValidator<ChargeCommand, ChargeOperationComposite>> businessValidator, ChargeOperationComposite anyComposite)
        {
            var validResult = ValidationResult.CreateSuccess();
            businessValidator
                .Setup(v => v.ValidateAsync(anyComposite))
                .Returns(Task.FromResult(validResult));
        }

        private static void ConfigureValidatorToReturnInvalidResult(
            Mock<IBusinessValidator<ChargeCommand, ChargeOperationComposite>> businessValidator, ChargeOperationComposite anyComposite)
        {
            var invalidResult = CreateInvalidValidationResult();
            businessValidator
                .Setup(v => v.ValidateAsync(anyComposite))
                .Returns(Task.FromResult(invalidResult));
        }

        private static ValidationResult CreateInvalidValidationResult()
        {
            var invalidRules = new List<IValidationRule> { new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation) };
            return ValidationResult.CreateFailure(invalidRules);
        }
    }
}
