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

using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class AuthenticatedUserMustMatchMarketParticipantSenderIdRule : IValidationRule
    {
        private readonly string? _authenticatedMarketParticipantId;
        private readonly string _marketParticipantSenderId;

        public AuthenticatedUserMustMatchMarketParticipantSenderIdRule(
            string? authenticatedMarketParticipantId,
            string marketParticipantSenderId)
        {
            _authenticatedMarketParticipantId = authenticatedMarketParticipantId;
            _marketParticipantSenderId = marketParticipantSenderId;
        }

        public bool IsValid =>
            _authenticatedMarketParticipantId is not null &&
            _authenticatedMarketParticipantId == _marketParticipantSenderId;

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.AuthenticatedUserMustMatchMarketParticipantSenderIdRule;
    }
}
