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

using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.InputValidation.Factories
{
    [UnitTest]
    public class ChargeLinksCommandInputValidatorTests
    {
        [Theory]
        [InlineAutoData]
        public void Validate_WhenValidatingChargeCommand_ReturnsChargeCommandValidationResult(
            ChargeLinksCommandInputValidationRulesFactory chargeLinksCommandInputValidationRulesFactory,
            ChargeLinksCommand chargeLinksCommand)
        {
            // Arrange
            var sut = new InputValidator<ChargeLinksCommand>(chargeLinksCommandInputValidationRulesFactory);

            // Act
            var result = sut.Validate(chargeLinksCommand);

            // Assert
            Assert.IsType<ValidationResult>(result);
        }
    }
}
