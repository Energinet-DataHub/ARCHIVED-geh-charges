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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices
{
    public class ChargeMessageQueryService : IChargeMessageQueryService
    {
        private readonly IData _data;
        private readonly Iso8601Durations _iso8601Durations;

        public ChargeMessageQueryService(IData data, Iso8601Durations iso8601Durations)
        {
            _data = data;
            _iso8601Durations = iso8601Durations;
        }

        public async Task<ChargeMessagesV1Dto> GetAsync(ChargeMessagesSearchCriteriaV1Dto chargeMessagesSearchCriteriaV1Dto)
        {
            var charge = await _data.Charges
                .SingleOrDefaultAsync(c => c.Id == chargeMessagesSearchCriteriaV1Dto.ChargeId)
                .ConfigureAwait(false);

            if (charge == null) throw new ArgumentException("Charge not found");

            var marketParticipant = await _data.MarketParticipants.SingleAsync(mp => mp.Id == charge.OwnerId!).ConfigureAwait(false);

            var chargeMessages = _data.ChargeMessages
                .Where(cm => cm.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                             cm.Type == charge.Type &&
                             cm.MarketParticipantId == marketParticipant.MarketParticipantId);
                //.Where(cm => cm. Time >= searchCriteria.FromDateTime && c.Time < searchCriteria.ToDateTime);
            return new ChargeMessagesV1Dto(charge.Id, chargeMessages.Select(x => x.MessageId).ToList());

            /*return await chargeMessages
                .AsChargeMessageV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);*/
        }
    }
}
