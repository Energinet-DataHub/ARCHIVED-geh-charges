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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Messaging;
using GreenEnergyHub.Queues.ValidationReportDispatcher.Validation;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    public class ChangeOfChargeTransactionInputValidator : IChangeOfChargeTransactionInputValidator
    {
        private readonly IRuleEngine<ChangeOfChargesTransaction> _inputValidationRuleEngine;

        public ChangeOfChargeTransactionInputValidator(IRuleEngine<ChangeOfChargesTransaction> inputValidationRuleEngine)
        {
            _inputValidationRuleEngine = inputValidationRuleEngine;
        }

        public async Task<HubRequestValidationResult> ValidateAsync([NotNull] ChangeOfChargesTransaction changeOfChargesMessage)
        {
            var result = await _inputValidationRuleEngine.ValidateAsync(changeOfChargesMessage).ConfigureAwait(false);

            var hubRequestValidationResult = new HubRequestValidationResult(changeOfChargesMessage.Transaction.MRID);

            foreach (var error in result)
            {
                hubRequestValidationResult.Add(new ValidationError(error.RuleNumber, error.Message));
            }

            return hubRequestValidationResult;
        }
    }
}
