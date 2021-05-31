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
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Factories
{
    public class BusinessUpdateValidationRulesFactory : IBusinessUpdateValidationRulesFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IZonedDateTimeService _localDateTimeService;
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public BusinessUpdateValidationRulesFactory(
            IChargeRepository chargeRepository,
            IRulesConfigurationRepository rulesConfigurationRepository,
            IZonedDateTimeService localDateTimeService,
            IMarketParticipantRepository marketParticipantRepository)
        {
            _chargeRepository = chargeRepository;
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _localDateTimeService = localDateTimeService;
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesForUpdateCommandAsync([NotNull] ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var chargeId = chargeCommand.ChargeOperation.ChargeId;
            var chargeOperationChargeOwner = chargeCommand.ChargeOperation.ChargeOwner;
            var chargeType = chargeCommand.ChargeOperation.Type;

            var charge = await _chargeRepository.GetChargeAsync(
                chargeId,
                chargeOperationChargeOwner,
                chargeType).ConfigureAwait(false);

            if (charge == null)
            {
                throw new Exception($"Charge not found on ChargeId: {chargeId}, ChargeOwner: {chargeOperationChargeOwner}, ChargeType: {chargeType}");
            }

            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var senderId = chargeCommand.Document.Sender.Id;
            var sender = _marketParticipantRepository.GetMarketParticipantOrNull(senderId);

            var rules = GetRules(chargeCommand, configuration, charge, sender);

            return ValidationRuleSet.FromRules(rules);
        }

        private List<IValidationRule> GetRules(
            ChargeCommand command,
            RulesConfiguration configuration,
            Charge charge,
            MarketParticipant? sender)
        {
            var rules = new List<IValidationRule>
            {
                new StartDateValidationRule(
                    command,
                    configuration.StartDateValidationRuleConfiguration,
                    _localDateTimeService),
                new CommandSenderMustBeAnExistingMarketParticipantRule(sender),
            };

            if (command.ChargeOperation.Type == ChargeType.Tariff)
            {
                AddTariffOnlyRules(rules, command, charge);
            }

            return rules;
        }

        private static void AddTariffOnlyRules(
            List<IValidationRule> rules,
            ChargeCommand command,
            Charge charge)
        {
            rules.Add(new ChangingTariffVatValueNotAllowedRule(command, charge));
            rules.Add(new ChangingTariffTaxValueNotAllowedRule(command, charge));
        }
    }
}
