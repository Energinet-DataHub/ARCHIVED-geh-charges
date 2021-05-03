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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessValidationRulesFactory : IBusinessValidationRulesFactory
    {
        private readonly IBusinessUpdateValidationRulesFactory _businessUpdateValidationRulesFactory;
        private readonly IBusinessAdditionValidationRulesFactory _businessAdditionValidationRulesFactory;

        public BusinessValidationRulesFactory(
            IBusinessUpdateValidationRulesFactory businessUpdateValidationRulesFactory,
            IBusinessAdditionValidationRulesFactory businessAdditionValidationRulesFactory)
        {
            _businessUpdateValidationRulesFactory = businessUpdateValidationRulesFactory;
            _businessAdditionValidationRulesFactory = businessAdditionValidationRulesFactory;
        }

        public async Task<IValidationRuleSet> CreateRulesForChargeCommandAsync(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            return chargeCommand.MktActivityRecord!.Status switch
            {
                MktActivityRecordStatus.Addition => await _businessAdditionValidationRulesFactory.CreateRulesForAdditionCommandAsync(chargeCommand).ConfigureAwait(false),
                MktActivityRecordStatus.Change => await _businessUpdateValidationRulesFactory.CreateRulesForUpdateCommandAsync(chargeCommand).ConfigureAwait(false),
                _ => throw new NotImplementedException("Unknown operation"),
            };
        }
    }
}
