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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation.DocumentValidation.ValidationRules
{
    public class SenderMustMatchChargeOwnersRule : IValidationRule
    {
        private readonly DocumentDto _documentDto;
        private readonly IEnumerable<string> _chargeOwners;

        public SenderMustMatchChargeOwnersRule(DocumentDto documentDto, IEnumerable<string> chargeOwners)
        {
            _documentDto = documentDto;
            _chargeOwners = chargeOwners;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.SenderMustMatchChargeOwners;

        public bool IsValid => _chargeOwners.All(chargeOwner => chargeOwner == _documentDto.Sender.MarketParticipantId);
    }
}
