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
using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class SearchCriteriaDtoBuilder
    {
        private string _chargeIdOrName = string.Empty;
        private Guid? _marketParticipantId = null;
        private List<ChargeType> _chargeTypes = new List<ChargeType>();

        public SearchCriteriaDtoBuilder WithChargeIdOrName(string chargeIdOrName)
        {
            _chargeIdOrName = chargeIdOrName;
            return this;
        }

        public SearchCriteriaDtoBuilder WithMarketParticipantId(Guid marketParticipantId)
        {
            _marketParticipantId = marketParticipantId;
            return this;
        }

        public SearchCriteriaDtoBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeTypes = new List<ChargeType> { chargeType };
            return this;
        }

        public SearchCriteriaDtoBuilder WithChargeTypes(List<ChargeType> chargeTypes)
        {
            _chargeTypes = chargeTypes;
            return this;
        }

        public SearchCriteriaV1Dto Build()
        {
            return new SearchCriteriaV1Dto(_chargeIdOrName, _marketParticipantId, _chargeTypes);
        }
    }
}
