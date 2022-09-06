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

using System;

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public static class ValidationErrorFactory
    {
        public static Func<IValidationRuleContainer, ValidationError> Create()
        {
            return rule =>
            {
                if (rule is IOperationValidationRuleContainer operationValidationRuleContainer &&
                    rule.ValidationRule is IValidationRuleWithExtendedData validationRuleWithExtendedData)
                {
                    return new ValidationError(
                        rule.ValidationRule.ValidationRuleIdentifier,
                        operationValidationRuleContainer.OperationId,
                        validationRuleWithExtendedData.TriggeredBy);
                }

                if (rule is IOperationValidationRuleContainer validationRuleForOperation)
                {
                    return new ValidationError(
                        rule.ValidationRule.ValidationRuleIdentifier,
                        validationRuleForOperation.OperationId,
                        null!);
                }

                return new ValidationError(rule.ValidationRule.ValidationRuleIdentifier, null!, null!);
            };
        }
    }
}
