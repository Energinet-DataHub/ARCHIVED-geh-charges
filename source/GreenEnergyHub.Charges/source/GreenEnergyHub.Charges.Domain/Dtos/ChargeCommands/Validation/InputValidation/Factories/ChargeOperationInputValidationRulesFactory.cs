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

        private static IList<ValidationRuleContainer> GetRulesForOperation(ChargeOperationDto chargeOperationDto)
        {
            var rules = new List<ValidationRuleContainer>
            {
                new(new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto.Id),
                new(new MaximumPriceRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
            };

            return rules;
        }
    }
}
