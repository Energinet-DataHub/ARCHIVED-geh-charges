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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessUpdateValidationRulesFactory : IBusinessUpdateValidationRulesFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IZonedDateTimeService _localDateTimeService;

        public BusinessUpdateValidationRulesFactory(
            IChargeRepository chargeRepository,
            IRulesConfigurationRepository rulesConfigurationRepository,
            IZonedDateTimeService localDateTimeService)
        {
            _chargeRepository = chargeRepository;
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _localDateTimeService = localDateTimeService;
        }

        public async Task<IBusinessValidationRuleSet> CreateRulesForUpdateCommandAsync([NotNull] ChargeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var chargeTypeMRid = command.ChargeTypeMRid!;
            var commandChargeTypeOwnerMRid = command.ChargeTypeOwnerMRid!;

            var charge = await _chargeRepository.GetChargeAsync(chargeTypeMRid, commandChargeTypeOwnerMRid).ConfigureAwait(false);

            if (charge == null)
            {
                throw new Exception($"Charge found on MRid: {chargeTypeMRid}, ChargeTypeOwnerMRid: {commandChargeTypeOwnerMRid}");
            }

            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var rules = GetRules(command, configuration, charge);

            return BusinessValidationRuleSet.FromRules(rules);
        }

        private List<IBusinessValidationRule> GetRules(ChargeCommand command, RulesConfiguration configuration, Charge charge)
        {
            var rules = new List<IBusinessValidationRule>
            {
                new StartDateVr209ValidationRule(
                    command,
                    configuration.StartDateVr209ValidationRuleConfiguration,
                    _localDateTimeService),
            };

            if (command.Type == ChargeCommandType.Tariff)
            {
                AddTariffOnlyRules(rules, command, charge);
            }

            return rules;
        }

        private static void AddTariffOnlyRules(
            List<IBusinessValidationRule> rules,
            ChargeCommand command,
            Charge charge)
        {
            rules.Add(new VatPayerMustNotChangeInUpdateRule(command, charge));
            rules.Add(new TaxIndicatorMustNotChangeInUpdateRule(command, charge));
        }
    }
}
