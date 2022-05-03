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
    public class ChargeCommandInputValidationRulesFactory : IInputValidationRulesFactory<ChargeOperationDto>
    {
        public IValidationRuleSet CreateRulesForOperation(ChargeOperationDto chargeOperation)
        {
            ArgumentNullException.ThrowIfNull(chargeOperation);

            var rules = GetRulesForOperation(chargeOperation).ToList();

            return ValidationRuleSet.FromRules(rules);
        }

        private static List<IValidationRule> GetRulesForOperation(ChargeOperationDto chargeOperationDto)
        {
            var rules = new List<IValidationRule>
            {
                new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto),
                new ChargeIdLengthValidationRule(chargeOperationDto),
                new ChargeIdRequiredValidationRule(chargeOperationDto),
                new ChargeNameHasMaximumLengthRule(chargeOperationDto),
                new ChargeOperationIdRequiredRule(chargeOperationDto),
                new ChargeOwnerIsRequiredValidationRule(chargeOperationDto),
                new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto),
                new ChargeTypeIsKnownValidationRule(chargeOperationDto),
                new ChargeTypeTariffPriceCountRule(chargeOperationDto),
                new MaximumPriceRule(chargeOperationDto),
                new ResolutionFeeValidationRule(chargeOperationDto),
                new ResolutionSubscriptionValidationRule(chargeOperationDto),
                new ResolutionTariffValidationRule(chargeOperationDto),
                new StartDateTimeRequiredValidationRule(chargeOperationDto),
                new VatClassificationValidationRule(chargeOperationDto),
                new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto),
            };

            return rules;
        }
    }
}
