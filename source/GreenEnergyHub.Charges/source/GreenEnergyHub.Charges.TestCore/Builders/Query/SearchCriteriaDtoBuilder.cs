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
using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class SearchCriteriaDtoBuilder
    {
        private string _chargeIdOrName = string.Empty;
        private string _marketParticipantId = string.Empty;
        private string _chargeTypes = string.Empty;

        public SearchCriteriaDtoBuilder WithChargeIdOrName(string chargeIdOrName)
        {
            _chargeIdOrName = chargeIdOrName;
            return this;
        }

        public SearchCriteriaDtoBuilder WithMarketParticipantId(string marketParticipantId)
        {
            _marketParticipantId = marketParticipantId;
            return this;
        }

        public SearchCriteriaDtoBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeTypes = chargeType.ToString();
            return this;
        }

        public SearchCriteriaDtoBuilder WithChargeTypes(List<ChargeType> chargeTypes)
        {
            _chargeTypes = string.Join(",", chargeTypes.Select(type => type.ToString()));
            return this;
        }

        public SearchCriteriaDto Build()
        {
            return new SearchCriteriaDto(_chargeIdOrName, _marketParticipantId, _chargeTypes);
        }
    }
}
