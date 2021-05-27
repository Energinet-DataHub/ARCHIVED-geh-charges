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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Factories
{
    public class BusinessCreateValidationRulesFactory : IBusinessCreateValidationRulesFactory
    {
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IZonedDateTimeService _zonedDateTimeService;
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public BusinessCreateValidationRulesFactory(
            IRulesConfigurationRepository rulesConfigurationRepository,
            IChargeRepository chargeRepository,
            IZonedDateTimeService zonedDateTimeService,
            IMarketParticipantRepository marketParticipantRepository)
        {
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _chargeRepository = chargeRepository;
            _zonedDateTimeService = zonedDateTimeService;
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesForCreateCommandAsync([NotNull] ChargeCommand chargeCommand)
        {
            await CheckIfChargeExistAsync(chargeCommand).ConfigureAwait(false);
            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var senderId = chargeCommand.Document.Sender.Id;
            var sender = _marketParticipantRepository.GetMarketParticipantOrNull(senderId);

            var rules = GetRules(chargeCommand, configuration, sender);

            return ValidationRuleSet.FromRules(rules);
        }

        private List<IValidationRule> GetRules(
            ChargeCommand command,
            RulesConfiguration configuration,
            MarketParticipant? sender)
        {
            var rules = new List<IValidationRule>
            {
                new StartDateValidationRule(
                    command,
                    configuration.StartDateValidationRuleConfiguration,
                    _zonedDateTimeService),
                new CommandSenderMustBeAnExistingMarketParticipantRule(sender),
            };

            return rules;
        }

        private async Task CheckIfChargeExistAsync(ChargeCommand command)
        {
            var chargeTypeMRid = command.ChargeOperation.Id;
            var commandChargeTypeOwnerMRid = command.ChargeOperation.ChargeOwner;
            var chargeType = command.ChargeOperation.Type;

            var result = await _chargeRepository.CheckIfChargeExistsAsync(
                chargeTypeMRid,
                commandChargeTypeOwnerMRid,
                chargeType).ConfigureAwait(false);

            if (result)
            {
                throw new Exception($"Charge found on MRid: {chargeTypeMRid}, ChargeTypeOwnerMRid: {commandChargeTypeOwnerMRid}, ChargeType: {chargeType}");
            }
        }
    }
}
