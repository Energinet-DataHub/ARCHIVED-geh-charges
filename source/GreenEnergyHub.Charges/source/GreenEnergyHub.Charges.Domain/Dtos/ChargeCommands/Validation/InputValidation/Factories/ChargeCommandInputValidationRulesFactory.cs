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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories
{
    public class ChargeCommandInputValidationRulesFactory : IInputValidationRulesFactory<ChargeCommand>
    {
        public IValidationRuleSet CreateRulesForCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = new List<IValidationRule>();
            foreach (var chargeDto in chargeCommand.Charges)
            {
                rules = GetRules(chargeDto);
            }

            return ValidationRuleSet.FromRules(rules);
        }

        private static List<IValidationRule> GetRules(ChargeDto chargeDto)
        {
            var rules = new List<IValidationRule>
            {
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(chargeDto),
                new ChargeDescriptionHasMaximumLengthRule(chargeDto),
                new ChargeIdLengthValidationRule(chargeDto),
                new ChargeIdRequiredValidationRule(chargeDto),
                new ChargeNameHasMaximumLengthRule(chargeDto),
                new ChargeOperationIdRequiredRule(chargeDto),
                new ChargeOwnerIsRequiredValidationRule(chargeDto),
                new ChargePriceMaximumDigitsAndDecimalsRule(chargeDto),
                new ChargeTypeIsKnownValidationRule(chargeDto),
                new ChargeTypeTariffPriceCountRule(chargeDto),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(chargeDto),
                new FeeMustHaveSinglePriceRule(chargeDto),
                new MaximumPriceRule(chargeDto),
                new RecipientIsMandatoryTypeValidationRule(chargeDto),
                new ResolutionFeeValidationRule(chargeDto),
                new ResolutionSubscriptionValidationRule(chargeDto),
                new ResolutionTariffValidationRule(chargeDto),
                new SenderIsMandatoryTypeValidationRule(chargeDto),
                new StartDateTimeRequiredValidationRule(chargeDto),
                new StopChargeNotYetSupportedValidationRule(chargeDto),
                new SubscriptionMustHaveSinglePriceRule(chargeDto),
                new VatClassificationValidationRule(chargeDto),
            };

            return rules;
        }
    }
}
