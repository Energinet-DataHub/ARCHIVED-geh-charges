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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.Factories
{
    public class ChargeCommandDocumentValidationRulesFactory : IValidationRulesFactory<ChargeCommand>
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeCommandDocumentValidationRulesFactory(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var document = chargeCommand.Document;

            if (document == null) throw new NullReferenceException(nameof(document));

            var rules = await GetRulesForDocumentAsync(document).ConfigureAwait(false);

            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRule>> GetRulesForDocumentAsync(DocumentDto documentDto)
        {
            var sender = await _marketParticipantRepository
                .GetOrNullAsync(documentDto.Sender.Id)
                .ConfigureAwait(false);

            var rules = new List<IValidationRule>
            {
                new CommandSenderMustBeAnExistingMarketParticipantRule(sender),
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(documentDto),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(documentDto),
                new RecipientIsMandatoryTypeValidationRule(documentDto),
                new SenderIsMandatoryTypeValidationRule(documentDto),
            };

            return rules;
        }
    }
}
