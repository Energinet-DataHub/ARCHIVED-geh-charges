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

            var rules = new List<IValidationRule>();

            foreach (var chargeDto in chargeCommand.Charges)
            {
                var senderId = chargeDto.Document.Sender.Id;
                var sender = await _marketParticipantRepository.GetOrNullAsync(senderId).ConfigureAwait(false);
                var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

                var charge = await GetChargeOrNullAsync(chargeDto).ConfigureAwait(false);
                var rulesForDto = GetMandatoryRules(chargeDto, configuration, sender);

                if (charge != null)
                {
                    if (chargeDto.ChargeOperation.Type == ChargeType.Tariff)
                        AddTariffOnlyRules(rulesForDto, chargeDto, charge);
                }

                rules.AddRange(rulesForDto);
            }

            return ValidationRuleSet.FromRules(rules);
        }

        private static void AddTariffOnlyRules(ICollection<IValidationRule> rules, ChargeDto chargeDto, Charge charge)
        {
            rules.Add(new ChangingTariffTaxValueNotAllowedRule(chargeDto, charge));
        }

        private List<IValidationRule> GetMandatoryRules(
            ChargeDto chargeDto,
            RulesConfiguration configuration,
            MarketParticipant? sender)
        {
            var rules = new List<IValidationRule>
            {
                new StartDateValidationRule(
                    chargeDto,
                    configuration.StartDateValidationRuleConfiguration,
                    _zonedDateTimeService,
                    _clock),
                new CommandSenderMustBeAnExistingMarketParticipantRule(sender),
            };

            return rules;
        }

        private Task<Charge?> GetChargeOrNullAsync(ChargeDto chargeDto)
        {
            var chargeIdentifier = new ChargeIdentifier(
                chargeDto.ChargeOperation.ChargeId,
                chargeDto.ChargeOperation.ChargeOwner,
                chargeDto.ChargeOperation.Type);

            return _chargeRepository.GetOrNullAsync(chargeIdentifier);
        }
    }
}
