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

using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    public class SearchCriteriaDtoBuilder
    {
        private string _chargeIdOrName = string.Empty;
        private string _marketParticipantId = string.Empty;
        private string _chargeType = string.Empty;
        private string _validity = string.Empty;

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
            _chargeType = chargeType.ToString();
            return this;
        }

        public SearchCriteriaDtoBuilder WithValidity(ValidityOptions validity)
        {
            _validity = validity.ToString();
            return this;
        }

        public SearchCriteriaDto Build()
        {
            return new SearchCriteriaDto(_chargeIdOrName, _marketParticipantId, _chargeType, _validity);
        }
    }
}
