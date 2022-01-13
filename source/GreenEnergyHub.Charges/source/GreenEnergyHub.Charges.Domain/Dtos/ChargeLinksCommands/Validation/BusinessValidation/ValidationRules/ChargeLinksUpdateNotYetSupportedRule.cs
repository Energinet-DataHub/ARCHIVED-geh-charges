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

using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules
{
    /// <summary>
    /// Temporary rule that stops both update and stops from taking place to charge links until that is implemented
    /// </summary>
    public class ChargeLinksUpdateNotYetSupportedRule : IValidationRule
    {
        private readonly ChargeLinksCommand _chargeLinksCommand;
        private readonly IReadOnlyCollection<ChargeLink> _existingChargeLinks;

        public ChargeLinksUpdateNotYetSupportedRule(ChargeLinksCommand chargeLinksCommand, IReadOnlyCollection<ChargeLink> existingChargeLinks)
        {
            _chargeLinksCommand = chargeLinksCommand;
            _existingChargeLinks = existingChargeLinks;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.UpdateNotYetSupported;

        public bool IsValid => _chargeLinksCommand.ChargeLinks.All(ChargeLinkDateRangeIsNotOverlapping);

        public string? TriggeredBy => null;

        private bool ChargeLinkDateRangeIsNotOverlapping(ChargeLinkDto newLink)
        {
            foreach (var existingLink in _existingChargeLinks)
            {
                // See https://stackoverflow.com/questions/325933/determine-whether-two-date-ranges-overlap
                if (existingLink.StartDateTime < newLink.EndDateTime && existingLink.EndDateTime > newLink.StartDateTime)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
