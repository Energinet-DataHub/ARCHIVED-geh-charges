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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
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
            [Frozen] Mock<IInputValidationRulesFactory<ChargeOperationDto>> inputValidationRulesFactory,
            InputValidator<ChargeOperationDto> sut,
            ChargeOperationDto chargeOperationDto)
        {
            // Arrange
            var testValidationRule = new TestValidationRule(isValid, ValidationRuleIdentifier.StartDateValidation);
            var rules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(testValidationRule, chargeOperationDto.Id),
                new OperationValidationRuleContainer(testValidationRule, chargeOperationDto.Id),
            };
            var validationRuleSet = ValidationRuleSet.FromRules(rules);
            inputValidationRulesFactory
                .Setup(f => f.CreateRules(It.IsAny<ChargeOperationDto>()))
                .Returns(validationRuleSet);

            // Act
            var actual = sut.Validate(chargeOperationDto);

            // Assert
            var shouldFail = !isValid;
            Assert.Equal(shouldFail, actual.IsFailed);
        }
    }
}
