﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules
{
    /// <summary>
    /// Temporary rule that stops both update and stops from taking place to charge links until that is implemented
    /// </summary>
    public class ChargeLinksUpdateNotYetSupportedRule : IValidationRuleWithExtendedData
    {
        private readonly ChargeLinksCommand _chargeLinksCommand;
        private readonly IReadOnlyCollection<ChargeLink> _existingChargeLinks;

        public ChargeLinksUpdateNotYetSupportedRule(
            ChargeLinksCommand chargeLinksCommand,
            IReadOnlyCollection<ChargeLink> existingChargeLinks)
        {
            _chargeLinksCommand = chargeLinksCommand;
            _existingChargeLinks = existingChargeLinks;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.UpdateNotYetSupported;

        public bool IsValid => _chargeLinksCommand.ChargeLinks.All(ChargeLinkDateRangeIsNotOverlapping);

        private bool ChargeLinkDateRangeIsNotOverlapping(ChargeLinkDto newLink)
        {
            return _existingChargeLinks.All(existingLink => !IsOverlapping(existingLink, newLink));
        }

        private static bool IsOverlapping(ChargeLink existingLink, ChargeLinkDto newLink)
        {
            var newLinkEndDateTime = newLink.EndDateTime.TimeOrEndDefault();

            // See https://stackoverflow.com/questions/325933/determine-whether-two-date-ranges-overlap
            var isOverlapping = existingLink.StartDateTime < newLinkEndDateTime
                                && newLink.StartDateTime < existingLink.EndDateTime;

            return isOverlapping;
        }

        /// <summary>
        /// This validation rule validates each ChargeLink in a list of ChargeLink(s). This property will
        /// tell which ChargeLink triggered the rule. The ChargeLink is identified by SenderProvidedChargeId.
        /// </summary>
        public string TriggeredBy => _chargeLinksCommand.ChargeLinks
            .First(link => !ChargeLinkDateRangeIsNotOverlapping(link)).SenderProvidedChargeId;
    }
}
