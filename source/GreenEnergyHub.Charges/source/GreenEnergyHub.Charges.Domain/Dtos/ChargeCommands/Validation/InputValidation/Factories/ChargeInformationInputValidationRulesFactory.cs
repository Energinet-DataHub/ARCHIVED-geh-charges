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
    public class ChargeInformationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeInformationDto>
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
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeInformationDto), chargeInformationDto),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeInformationDto), chargeInformationDto),
            };

            rules.AddRange(CreateRulesForChargeInformation(chargeInformationDto));

            return rules;
        }

        private static List<IValidationRuleContainer> CreateRulesForChargeInformation(ChargeInformationDto chargeOperationDto)
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
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto),
#pragma warning disable CS0618
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto),
#pragma warning restore CS0618
            };
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, ChargeOperation chargeInformationDto)
        {
            return new OperationValidationRuleContainer(validationRule, chargeInformationDto.Id);
        }
    }
}
