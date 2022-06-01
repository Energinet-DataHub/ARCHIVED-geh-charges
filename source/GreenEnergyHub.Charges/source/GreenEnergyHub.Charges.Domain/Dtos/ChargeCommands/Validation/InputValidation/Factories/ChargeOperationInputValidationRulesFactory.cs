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

        private static IEnumerable<IValidationRuleContainer> GetRulesForOperation(ChargeInformationDto chargeInformationDto)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto),
            };

            rules.AddRange(chargeOperationDto.Points.Any()
                ? CreateRulesForChargePrice(chargeOperationDto)
                : CreateRulesForChargeInformation(chargeOperationDto));

            return rules;
        }

        private static List<IValidationRuleContainer> CreateRulesForChargePrice(ChargeOperationDto chargeOperationDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto),
            };
        }

        private static List<IValidationRuleContainer> CreateRulesForChargeInformation(ChargeOperationDto chargeOperationDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto),
            };
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, ChargeInformationDto chargeInformationDto)
        {
            return new OperationValidationRuleContainer(validationRule, chargeInformationDto.Id);
        }
    }
}
