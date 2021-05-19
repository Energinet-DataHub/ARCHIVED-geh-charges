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
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.InputValidation
{
    public class InputValidationRulesFactory : IInputValidationRulesFactory
    {
        public IValidationRuleSet CreateRulesForChargeCreateCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = GetCreateRules(chargeCommand);
            rules.AddRange(GetMandatoryRules(chargeCommand));

            return ValidationRuleSet.FromRules(rules);
        }

        public IValidationRuleSet CreateRulesForChargeUpdateCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = GetUpdateRules(chargeCommand);
            rules.AddRange(GetMandatoryRules(chargeCommand));

            return ValidationRuleSet.FromRules(rules);
        }

        public IValidationRuleSet CreateRulesForChargeStopCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var mandatoryRules = GetMandatoryRules(chargeCommand);

            return ValidationRuleSet.FromRules(mandatoryRules);
        }

        public IValidationRuleSet CreateRulesForChargeUnknownCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var rules = new List<IValidationRule>
            {
                new OperationTypeValidationRule(chargeCommand),
            };

            return ValidationRuleSet.FromRules(rules);
        }

        private static List<IValidationRule> GetCreateRules(ChargeCommand chargeCommand)
        {
            var rules = new List<IValidationRule>
            {
                new ProcessTypeIsKnownValidationRule(chargeCommand),
                new SenderIsMandatoryTypeValidationRule(chargeCommand),
                new RecipientIsMandatoryTypeValidationRule(chargeCommand),
                new VatClassificationValidationRule(chargeCommand),
                new ResolutionTariffValidationRule(chargeCommand),
                new ResolutionFeeValidationRule(chargeCommand),
                new ResolutionSubscriptionValidationRule(chargeCommand),
                new ChargeNameHasMaximumLengthRule(chargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(chargeCommand),
                new ChargeFeeSubscriptionPriceCountRule(chargeCommand),
            };

            return rules;
        }

        private static List<IValidationRule> GetUpdateRules(ChargeCommand chargeCommand)
        {
            var rules = new List<IValidationRule>
            {
                new ChargeNameHasMaximumLengthRule(chargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(chargeCommand),
                new ChargeFeeSubscriptionPriceCountRule(chargeCommand),
            };

            return rules;
        }

        private static List<IValidationRule> GetMandatoryRules(ChargeCommand chargeCommand)
        {
            var rules = new List<IValidationRule>
            {
                new ChargeOperationIdRequiredRule(chargeCommand),
                new ChargeIdRequiredValidationRule(chargeCommand),
                new BusinessReasonCodeMustBeUpdateChargeInformation(chargeCommand),
                new DocumentTypeMustBeRequestUpdateChargeInformation(chargeCommand),
                new ChargeTypeIsKnownValidationRule(chargeCommand),
                new ChargeIdLengthValidationRule(chargeCommand),
                new StartDateTimeRequiredValidationRule(chargeCommand),
                new OperationTypeValidationRule(chargeCommand),
                new ChargeOwnerIsRequiredValidationRule(chargeCommand),
            };

            return rules;
        }
    }
}
