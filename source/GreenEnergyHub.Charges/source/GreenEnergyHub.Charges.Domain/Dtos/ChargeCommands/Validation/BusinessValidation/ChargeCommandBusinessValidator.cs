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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation
{
    public class ChargeCommandBusinessValidator : IChargeCommandBusinessValidator
    {
        private readonly IBusinessValidationRulesFactory _businessValidationRulesFactory;

        public ChargeCommandBusinessValidator(IBusinessValidationRulesFactory businessValidationRulesFactory)
        {
            _businessValidationRulesFactory = businessValidationRulesFactory;
        }

        public async Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command)
        {
            var ruleSet = await _businessValidationRulesFactory.CreateRulesForChargeCommandAsync(command).ConfigureAwait(false);
            return ruleSet.Validate();
        }
    }
}
