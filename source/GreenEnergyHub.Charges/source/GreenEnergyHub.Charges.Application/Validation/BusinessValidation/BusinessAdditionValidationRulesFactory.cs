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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessAdditionValidationRulesFactory : IBusinessAdditionValidationRulesFactory
    {
        private readonly IUpdateRulesConfigurationRepository _updateRulesConfigurationRepository;
        private readonly IChargeRepository _chargeRepository;

        public BusinessAdditionValidationRulesFactory(
            IUpdateRulesConfigurationRepository updateRulesConfigurationRepository,
            IChargeRepository chargeRepository)
        {
            _updateRulesConfigurationRepository = updateRulesConfigurationRepository;
            _chargeRepository = chargeRepository;
        }

        public async Task<IBusinessValidationRuleSet> CreateRulesForAdditionCommandAsync([NotNull] ChargeCommand command)
        {
            await CheckIfChargeExistAsync(command).ConfigureAwait(false);
            var configuration = await _updateRulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var rules = GetRules(command, configuration);

            return BusinessValidationRuleSet.FromRules(rules);
        }

        private static List<IBusinessValidationRule> GetRules(ChargeCommand command, UpdateRulesConfiguration configuration)
        {
            var rules = new List<IBusinessValidationRule>
            {
                new StartDateVr209ValidationRule(
                    command,
                    configuration.StartDateVr209ValidationRuleConfiguration),
            };

            return rules;
        }

        private async Task CheckIfChargeExistAsync(ChargeCommand command)
        {
            var chargeTypeMRid = command.ChargeTypeMRid!;
            var commandChargeTypeOwnerMRid = command.ChargeTypeOwnerMRid!;

            var result = await _chargeRepository.CheckIfChargeExistsAsync(chargeTypeMRid, commandChargeTypeOwnerMRid).ConfigureAwait(false);
            if (result)
            {
                throw new Exception($"Charge found on MRid: {chargeTypeMRid}, ChargeTypeOwnerMRid: {commandChargeTypeOwnerMRid}");
            }
        }
    }
}
