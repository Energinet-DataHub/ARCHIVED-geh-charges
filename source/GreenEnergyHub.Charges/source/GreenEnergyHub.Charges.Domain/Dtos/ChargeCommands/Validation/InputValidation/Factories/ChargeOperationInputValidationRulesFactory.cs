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
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories
{
    public class ChargeOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeOperationDto>
    {
        public IValidationRuleSet CreateRules(ChargeOperationDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var rules = GetRulesForOperation(operation).ToList();
            return ValidationRuleSet.FromRules(rules);
        }

        private static IList<IValidationRuleContainer> GetRulesForOperation(ChargeOperationDto chargeOperationDto)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto),
            };

            return rules;
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, ChargeOperationDto chargeOperationDto)
        {
            return new OperationValidationRuleContainer(validationRule, chargeOperationDto.Id);
        }
    }
}
