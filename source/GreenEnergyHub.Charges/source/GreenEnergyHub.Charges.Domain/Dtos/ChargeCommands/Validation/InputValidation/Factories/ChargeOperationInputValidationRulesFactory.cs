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
    public class ChargeOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeInformationDto>
    {
        public IValidationRuleSet CreateRules(ChargeInformationDto informationDto)
        {
            ArgumentNullException.ThrowIfNull(informationDto);
            var rules = GetRulesForOperation(informationDto).ToList();
            return ValidationRuleSet.FromRules(rules);
        }

        private static IList<IValidationRuleContainer> GetRulesForOperation(ChargeInformationDto chargeInformationDto)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeDescriptionHasMaximumLengthRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeNameHasMaximumLengthRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new MaximumPriceRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ResolutionFeeValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ResolutionSubscriptionValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ResolutionTariffValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new VatClassificationValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeInformationDto), chargeInformationDto),
            };

            return rules;
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, ChargeInformationDto chargeInformationDto)
        {
            return new OperationValidationRuleContainer(validationRule, chargeInformationDto.Id);
        }
    }
}
