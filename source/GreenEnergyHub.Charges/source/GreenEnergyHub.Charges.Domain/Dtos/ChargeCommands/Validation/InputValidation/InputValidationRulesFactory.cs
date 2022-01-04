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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation
{
    public class InputValidationRulesFactory : IInputValidationRulesFactory
    {
        public IValidationRuleSet CreateRulesForChargeCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = GetRules(chargeCommand);

            return ValidationRuleSet.FromRules(rules);
        }

        private static List<IValidationRule> GetRules(ChargeCommand chargeCommand)
        {
            var rules = new List<IValidationRule>
            {
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(chargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(chargeCommand),
                new ChargeIdLengthValidationRule(chargeCommand),
                new ChargeIdRequiredValidationRule(chargeCommand),
                new ChargeNameHasMaximumLengthRule(chargeCommand),
                new ChargeOperationIdRequiredRule(chargeCommand),
                new ChargeOwnerIsRequiredValidationRule(chargeCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(chargeCommand),
                new ChargeTypeIsKnownValidationRule(chargeCommand),
                new ChargeTypeTariffPriceCountRule(chargeCommand),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(chargeCommand),
                new FeeMustHaveSinglePriceRule(chargeCommand),
                new MaximumPriceRule(chargeCommand),
                new RecipientIsMandatoryTypeValidationRule(chargeCommand),
                new ResolutionFeeValidationRule(chargeCommand),
                new ResolutionSubscriptionValidationRule(chargeCommand),
                new ResolutionTariffValidationRule(chargeCommand),
                new SenderIsMandatoryTypeValidationRule(chargeCommand),
                new StartDateTimeRequiredValidationRule(chargeCommand),
                new SubscriptionMustHaveSinglePriceRule(chargeCommand),
                new VatClassificationValidationRule(chargeCommand),
            };

            return rules;
        }
    }
}
