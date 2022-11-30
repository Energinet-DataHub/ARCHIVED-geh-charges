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
using Energinet.DataHub.Charges.Contracts.ChargeHistory;
using GreenEnergyHub.Charges.TestCore.Data;

namespace GreenEnergyHub.Charges.TestCore.Builders.Query
{
    public class ChargeHistorySearchCriteriaV1DtoBuilder
    {
        private Guid _chargeId = SeededData.MarketParticipants.Provider8100000000030.Id;
        private DateTimeOffset _atDateTime = DateTimeOffset.Now;

        public ChargeHistorySearchCriteriaV1DtoBuilder WithChargeId(Guid chargeId)
        {
            _chargeId = chargeId;
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
                _atDateTime);
        }
    }
}
