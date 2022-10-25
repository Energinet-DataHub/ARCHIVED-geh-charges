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
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices
{
    public class ChargeMessageQueryService : IChargeMessageQueryService
    {
        private readonly IData _data;

        public ChargeMessageQueryService(IData data)
        {
            _data = data;
        }

        public async Task<ChargeMessagesV1Dto> GetAsync(ChargeMessagesSearchCriteriaV1Dto chargeMessagesSearchCriteriaV1Dto)
        {
            var charge = await _data.Charges
                .SingleOrDefaultAsync(c => c.Id == chargeMessagesSearchCriteriaV1Dto.ChargeId)
                .ConfigureAwait(false);

            if (charge == null) throw new ArgumentException("Charge not found");

            var chargeMessages = _data.ChargeMessages.Where(x =>
                x.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                x.Type == charge.Type &&
                x.MarketParticipantId == charge.Owner.MarketParticipantId);

            return new ChargeMessagesV1Dto(charge.Id, chargeMessages.Select(x => x.MessageId).ToList());

            /*return await chargeMessages
                .AsChargeMessageV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);*/
        }
    }
}
