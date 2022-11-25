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
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeHistory;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class ChargeHistorySearchCriteriaV1DtoBuilder
    {
        private string _chargeId = "TariffA";
        private ChargeType _chargeType = ChargeType.D03;
        private string _chargeOwner = "Owner";
        private DateTimeOffset _atDateTime = DateTimeOffset.Now;

        public ChargeHistorySearchCriteriaV1DtoBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeHistorySearchCriteriaV1DtoBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeType = chargeType;
            return this;
        }

        public ChargeHistorySearchCriteriaV1DtoBuilder WithChargeOwner(string chargeOwner)
        {
            _chargeOwner = chargeOwner;
            return this;
        }

        public ChargeHistorySearchCriteriaV1DtoBuilder WithAtDateTime(DateTimeOffset fromDateTime)
        {
            _atDateTime = fromDateTime;
            return this;
        }

        public ChargeHistorySearchCriteriaV1Dto Build()
        {
            return new ChargeHistorySearchCriteriaV1Dto(
                _chargeId,
                _chargeType,
                _chargeOwner,
                _atDateTime);
        }
    }
}
