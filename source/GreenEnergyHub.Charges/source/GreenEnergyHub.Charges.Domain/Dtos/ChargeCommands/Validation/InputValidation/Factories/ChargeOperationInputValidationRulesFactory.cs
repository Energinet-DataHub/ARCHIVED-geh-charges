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
                new ValidationRuleContainer(
                    new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(
                    new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(
                    new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationRuleContainer(
                    new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
            };

            return rules;
        }
    }
}
