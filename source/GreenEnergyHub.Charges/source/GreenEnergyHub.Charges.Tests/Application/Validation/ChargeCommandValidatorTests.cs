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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Validation
{
    public class ChargeCommandValidatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationFails_ReturnsInvalid(
            [NotNull] [Frozen] Mock<IChangeOfChargeTransactionInputValidator> inputValidator,
            [NotNull] ChargeCommandValidator sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            var invalidResult = CreateInvalidValidationResult();
            inputValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(invalidResult));

            // Act
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);

            // Assert
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationSucceedsAndBusinessValidationFails_ReturnsInvalid(
            [NotNull] [Frozen] Mock<IChangeOfChargeTransactionInputValidator> inputValidator,
            [NotNull][Frozen] Mock<IChargeCommandBusinessValidator> businessValidator,
            [NotNull] ChargeCommandValidator sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            var validResult = ChargeCommandValidationResult.CreateSuccess();
            var invalidResult = CreateInvalidValidationResult();
            inputValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(validResult));
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(invalidResult));

            // Act
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);

            // Assert
            Assert.True(actual.IsFailed);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ValidateAsync_WhenInputValidationSucceedsAndBusinessValidationSucceeds_ReturnsValid(
            [NotNull] [Frozen] Mock<IChangeOfChargeTransactionInputValidator> inputValidator,
            [NotNull][Frozen] Mock<IChargeCommandBusinessValidator> businessValidator,
            [NotNull] ChargeCommandValidator sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            var validResult = ChargeCommandValidationResult.CreateSuccess();
            var invalidResult = ChargeCommandValidationResult.CreateSuccess();
            inputValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(validResult));
            businessValidator
                .Setup(v => v.ValidateAsync(anyCommand))
                .Returns(Task.FromResult(invalidResult));

            // Act
            var actual = await sut.ValidateAsync(anyCommand).ConfigureAwait(false);

            // Assert
            Assert.False(actual.IsFailed);
        }

        private static ChargeCommandValidationResult CreateInvalidValidationResult()
        {
            var invalidRules = new List<IBusinessValidationRule> { new BusinessValidationRule(false) };
            return ChargeCommandValidationResult.CreateFailure(invalidRules);
        }
    }
}
