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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Messaging;

namespace GreenEnergyHub.Charges.Application.Validation.InputValidation
{
    public class ChangeOfChargeTransactionInputValidator : IChangeOfChargeTransactionInputValidator
    {
        private readonly IRuleEngine<ChargeCommand> _inputValidationRuleEngine;

        public ChangeOfChargeTransactionInputValidator(IRuleEngine<ChargeCommand> inputValidationRuleEngine)
        {
            _inputValidationRuleEngine = inputValidationRuleEngine;
        }

        public async Task<ChargeCommandValidationResult> ValidateAsync([NotNull] ChargeCommand chargeCommand)
        {
            var result = await _inputValidationRuleEngine.ValidateAsync(chargeCommand).ConfigureAwait(false);

            // Since we have refactored the former validation result, now both input and business validation uses
            // the same model. But one works with rule types and the other works with codes.
            // This is a temp fix to address this issue.
            var validationRules = result.Select(r => new BusinessValidationRule(
                false,
                (ValidationRule)Enum.Parse(typeof(ValidationRule), r.Message))).ToArray<IBusinessValidationRule>();

            return !result.Success
                ? ChargeCommandValidationResult.CreateFailure(validationRules)
                : ChargeCommandValidationResult.CreateSuccess();
        }
    }
}
