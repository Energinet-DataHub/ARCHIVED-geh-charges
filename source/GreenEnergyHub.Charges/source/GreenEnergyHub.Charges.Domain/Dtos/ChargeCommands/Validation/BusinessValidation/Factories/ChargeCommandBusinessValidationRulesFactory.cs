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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories
{
    public class ChargeCommandBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeCommand>
    {
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IZonedDateTimeService _zonedDateTimeService;
        private readonly IClock _clock;

        public ChargeCommandBusinessValidationRulesFactory(
            IRulesConfigurationRepository rulesConfigurationRepository,
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IZonedDateTimeService zonedDateTimeService,
            IClock clock)
        {
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _zonedDateTimeService = zonedDateTimeService;
            _clock = clock;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var senderId = chargeCommand.Document.Sender.Id;
            var sender = await _marketParticipantRepository.GetOrNullAsync(senderId);

            var charge = await GetChargeOrNullAsync(chargeCommand).ConfigureAwait(false);

            var rules = GetMandatoryRules(chargeCommand, charge, configuration, sender);

            if (charge == null)
                return ValidationRuleSet.FromRules(rules);

            if (chargeCommand.ChargeOperation.Type == ChargeType.Tariff)
                AddTariffOnlyRules(rules, chargeCommand, charge);

            return ValidationRuleSet.FromRules(rules);
        }

        private static void AddTariffOnlyRules(
            List<IValidationRule> rules,
            ChargeCommand command,
            Charge charge)
        {
            rules.Add(new ChangingTariffVatValueNotAllowedRule(command, charge));
            rules.Add(new ChangingTariffTaxValueNotAllowedRule(command, charge));
        }

        private List<IValidationRule> GetMandatoryRules(
            ChargeCommand chargeCommand,
            Charge? charge,
            RulesConfiguration configuration,
            MarketParticipant? sender)
        {
            var rules = new List<IValidationRule>
            {
                new StartDateValidationRule(
                    chargeCommand,
                    configuration.StartDateValidationRuleConfiguration,
                    _zonedDateTimeService,
                    _clock),
                new CommandSenderMustBeAnExistingMarketParticipantRule(chargeCommand, sender),
                new ChargeUpdateNotYetSupportedRule(charge),
            };

            return rules;
        }

        private Task<Charge?> GetChargeOrNullAsync(ChargeCommand command)
        {
            var chargeId = command.ChargeOperation.ChargeId;
            var chargeOperationChargeOwner = command.ChargeOperation.ChargeOwner;
            var chargeType = command.ChargeOperation.Type;

            return _chargeRepository
                .GetOrNullAsync(new ChargeIdentifier(
                    chargeId,
                    chargeOperationChargeOwner,
                    chargeType));
        }
    }
}
