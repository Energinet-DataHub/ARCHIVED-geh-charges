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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation
{
    /// <summary>
    /// Temporary rule that stops both update and stops from taking place to charges until that is implemented
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

        public bool IsValid => _chargeLinksCommand.ChargeLinks.All(ChargeLinkDateRangeIsNotOverlapping);

        public ValidationError? ValidationError { get; }

        private bool ChargeLinkDateRangeIsNotOverlapping(ChargeLinkDto chargeLinkDto)
        {
            foreach (var link in _existingChargeLinks)
            {
                if (link.StartDateTime < chargeLinkDto.EndDateTime && link.EndDateTime >= chargeLinkDto.StartDateTime)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
