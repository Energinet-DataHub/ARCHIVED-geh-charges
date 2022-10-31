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
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeLink;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using Energinet.DataHub.Charges.Contracts.ChargePrice;

namespace Energinet.DataHub.Charges.Clients.Charges
{
    /// <summary>
    /// Charge Links Client Interface
    /// </summary>
    public interface IChargesClient
    {
        /// <summary>
        /// Gets all charge links data for a given metering point.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.</param>
        /// <returns>A collection of Charge Link DTOs</returns>
        public Task<IList<ChargeLinkV1Dto>> GetChargeLinksAsync(string meteringPointId);

        /// <summary>
        /// Gets all charges
        /// </summary>
        /// <returns>A collection of Charge DTOs</returns>
        public Task<IList<ChargeV1Dto>> GetChargesAsync();

        /// <summary>
        /// Returns charges based on the search criteria.
        /// </summary>
        /// <returns>A collection of charges(Dtos)</returns>
        public Task<IList<ChargeV1Dto>> SearchChargesAsync(ChargeSearchCriteriaV1Dto searchCriteria);

        /// <summary>
        /// Gets all market participants.
        /// </summary>
        /// <returns>A collection of Market Participant DTOs</returns>
        public Task<IList<MarketParticipantV1Dto>> GetMarketParticipantsAsync();

        /// <summary>
        /// Returns charge prices based on the search criteria.
        /// </summary>
        /// <returns>A <see cref="ChargePricesV1Dto"/></returns>
        Task<ChargePricesV1Dto> SearchChargePricesAsync(ChargePricesSearchCriteriaV1Dto searchCriteria);

        /// <summary>
        /// Returns charge messages based on the search criteria.
        /// </summary>
        /// <returns>Charge messages(Dto) with collection of messages</returns>
        Task<ChargeMessagesV1Dto> SearchChargeMessagesAsync(ChargeMessagesSearchCriteriaV1Dto searchCriteria);
    }
}
