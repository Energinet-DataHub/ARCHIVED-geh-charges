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
using Energinet.DataHub.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class ChargeSearchCriteriaV1CriteriaDtoBuilder
    {
        private string _chargeIdOrName = string.Empty;
        private List<Guid> _ownerIds = new List<Guid>();
        private List<ChargeType> _chargeTypes = new List<ChargeType>();

        public ChargeSearchCriteriaV1CriteriaDtoBuilder WithChargeIdOrName(string chargeIdOrName)
        {
            _chargeIdOrName = chargeIdOrName;
            return this;
        }

        public ChargeSearchCriteriaV1CriteriaDtoBuilder WithOwnerId(Guid ownerId)
        {
            _ownerIds.Add(ownerId);
            return this;
        }

        public ChargeSearchCriteriaV1CriteriaDtoBuilder WithOwnerIds(List<Guid> ownerIds)
        {
            _ownerIds = ownerIds;
            return this;
        }

        public ChargeSearchCriteriaV1CriteriaDtoBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeTypes = new List<ChargeType> { chargeType };
            return this;
        }

        public ChargeSearchCriteriaV1CriteriaDtoBuilder WithChargeTypes(List<ChargeType> chargeTypes)
        {
            _chargeTypes = chargeTypes;
            return this;
        }

        public ChargeSearchCriteriaV1Dto Build()
        {
            return new ChargeSearchCriteriaV1Dto(_chargeIdOrName, _ownerIds, _chargeTypes);
        }
    }
}
