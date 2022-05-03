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
    public class InputValidatorTests
    {
        [Theory]
        [InlineAutoDomainData(false)]
        [InlineAutoDomainData(true)]
        public void Validate_WhenFactoryCreateInvalidRules_ThenInvalidValidationResult(
            bool isValid,
            [Frozen] Mock<IInputValidationRulesFactory<ChargeCommand>> inputValidationRulesFactory,
            InputValidator<ChargeCommand> sut,
            ChargeCommand anyCommand)
        {
            // Arrange
            var testValidationRule = new TestValidationRule(isValid, ValidationRuleIdentifier.StartDateValidation);
            var rules = new List<IValidationRule> { testValidationRule, testValidationRule };
            var validationRuleSet = ValidationRuleSet.FromRules(rules);
            inputValidationRulesFactory
                .Setup(f => f.CreateRulesForOperation(It.IsAny<ChargeCommand>()))
                .Returns(validationRuleSet);

            // Act
            var actual = sut.Validate(anyCommand);

            // Assert
            var shouldFail = !isValid;
            Assert.Equal(shouldFail, actual.IsFailed);
        }
    }
}
