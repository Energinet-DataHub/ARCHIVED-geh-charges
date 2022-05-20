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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Validation
{
    [UnitTest]
    public class ValidationErrorFactoryTests
    {
        [Fact]
        public void Create_WhenDifferentKindsOfRulesAreViolated_ValidationErrorResult()
        {
            // Arrange
            const string invalidChargeId = "InValidId123";
            const string validChargeId = "ValidId123";
            var rejectionRules = new List<IValidationRuleContainer>
            {
                new DocumentValidationRuleContainer(
                    new TestValidationRule(false, ValidationRuleIdentifier.RecipientRoleMustBeDdz)),
                new OperationValidationRuleContainer(
                    new TestValidationRule(false, ValidationRuleIdentifier.ChargeIdLengthValidation), invalidChargeId),
                new OperationValidationRuleContainer(
                    new TestValidationRuleWithExtendedData(
                        false, ValidationRuleIdentifier.SubsequentBundleOperationsFail, invalidChargeId),
                    validChargeId),
            };
            var validationResult = ValidationResult.CreateFailure(rejectionRules);

            // Act
            var validationErrors = validationResult.InvalidRules.Select(ValidationErrorFactory.Create()).ToList();

            // Assert
            var actualDocumentValidationError = validationErrors.Single(x =>
                x.ValidationRuleIdentifier == ValidationRuleIdentifier.RecipientRoleMustBeDdz);
            var actualOperationValidationError = validationErrors.Single(x =>
                x.ValidationRuleIdentifier == ValidationRuleIdentifier.ChargeIdLengthValidation);
            var actualExtendedDataValidationError = validationErrors.Single(x =>
                x.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);

            actualDocumentValidationError.OperationId.Should().BeNull();
            actualDocumentValidationError.TriggeredBy.Should().BeNull();
            actualOperationValidationError.OperationId.Should().Be(invalidChargeId);
            actualOperationValidationError.TriggeredBy.Should().BeNull();
            actualExtendedDataValidationError.OperationId.Should().Be(validChargeId);
            actualExtendedDataValidationError.TriggeredBy.Should().Be(invalidChargeId);
        }
    }
}
