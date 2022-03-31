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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories
{
    public class ChargeCommandInputValidationRulesFactory : IInputValidationRulesFactory<ChargeCommand>
    {
        public IValidationRuleSet CreateRulesForCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = GetRulesForCommand(chargeCommand.Document);
            rules.AddRange(chargeCommand.Charges.SelectMany(GetRulesForOperation));

            return ValidationRuleSet.FromRules(rules);
        }

        private static List<IValidationRule> GetRulesForCommand(DocumentDto documentDto)
        {
            var rules = new List<IValidationRule>
            {
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(documentDto),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(documentDto),
                new RecipientIsMandatoryTypeValidationRule(documentDto),
                new SenderIsMandatoryTypeValidationRule(documentDto),
            };

            return rules;
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
                new FeeMustHaveSinglePriceRule(chargeOperationDto),
                new MaximumPriceRule(chargeOperationDto),
                new ResolutionFeeValidationRule(chargeOperationDto),
                new ResolutionSubscriptionValidationRule(chargeOperationDto),
                new ResolutionTariffValidationRule(chargeOperationDto),
                new StartDateTimeRequiredValidationRule(chargeOperationDto),
                new SubscriptionMustHaveSinglePriceRule(chargeOperationDto),
                new VatClassificationValidationRule(chargeOperationDto),
            };

            return rules;
        }
    }
}
