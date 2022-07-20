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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.DocumentValidation.Factories
{
    public class ChargeCommandDocumentValidationRulesFactory : IDocumentValidationRulesFactory
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeCommandDocumentValidationRulesFactory(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(CommandBase command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var document = command.Document;
            ArgumentNullException.ThrowIfNull(document);

            var rules = await GetRulesForDocumentAsync(document).ConfigureAwait(false);

            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRuleContainer>> GetRulesForDocumentAsync(DocumentDto documentDto)
        {
            var sender = await _marketParticipantRepository
                .SingleOrNullAsync(documentDto.Sender.BusinessProcessRole, documentDto.Sender.MarketParticipantId)
                .ConfigureAwait(false);

            var rules = new List<IValidationRuleContainer>
            {
                new DocumentValidationRuleContainer(new CommandSenderMustBeAnExistingMarketParticipantRule(sender)),
                new DocumentValidationRuleContainer(new BusinessReasonCodeMustBeUpdateChargeInformationOrChargePricesRule(documentDto)),
                new DocumentValidationRuleContainer(new DocumentTypeMustBeRequestChangeOfPriceListRule(documentDto)),
                new DocumentValidationRuleContainer(new RecipientIsMandatoryTypeValidationRule(documentDto)),
                new DocumentValidationRuleContainer(new SenderIsMandatoryTypeValidationRule(documentDto)),
                new DocumentValidationRuleContainer(new RecipientMustBeDdzRule(documentDto)),
            };

            return rules;
        }
    }
}
