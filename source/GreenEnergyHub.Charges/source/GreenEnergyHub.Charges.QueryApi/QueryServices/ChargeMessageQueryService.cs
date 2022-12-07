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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.QueryApi.Model;
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

        public async Task<ChargeMessagesV1Dto> SearchAsync(ChargeMessagesSearchCriteriaV1Dto searchCriteria)
        {
            var charge = await _data.Charges
                .SingleOrDefaultAsync(c => c.Id == searchCriteria.ChargeId)
                .ConfigureAwait(false);

            if (charge == null)
                throw new ArgumentNullException(nameof(charge), "Charge not found using the provided search criteria");

            var marketParticipant = await _data.MarketParticipants
                .SingleAsync(mp => mp.Id == charge.OwnerId!)
                .ConfigureAwait(false);

            var chargeMessages = QueryChargeMessages(searchCriteria, charge, marketParticipant);
            var takenChargeMessages = chargeMessages.Skip(searchCriteria.Skip).Take(searchCriteria.Take);
            var sortedChargeMessages = await SortChargeMessages(takenChargeMessages, searchCriteria)
                .ToListAsync().ConfigureAwait(false);

            return MapToChargeMessagesV1Dtos(sortedChargeMessages, chargeMessages.Count());
        }

        private static ChargeMessagesV1Dto MapToChargeMessagesV1Dtos(
            IEnumerable<ChargeMessage> chargeMessagesList, int totalCount)
        {
            var chargeMessagesV1Dto = new ChargeMessagesV1Dto(
                totalCount,
                chargeMessagesList.Select(cm =>
                    new ChargeMessageV1Dto(
                        cm.MessageId,
                        MapBusinessReasonCode((BusinessReasonCode)cm.MessageType),
                        DateTime.SpecifyKind(cm.MessageDateTime, DateTimeKind.Utc))));
            return chargeMessagesV1Dto;
        }

        private IQueryable<ChargeMessage> QueryChargeMessages(
            ChargeMessagesSearchCriteriaV1Dto searchCriteria,
            Charge charge,
            MarketParticipant marketParticipant)
        {
            return _data.ChargeMessages
                .Where(cm => cm.SenderProvidedChargeId == charge.SenderProvidedChargeId &&
                             cm.Type == charge.Type &&
                             cm.MarketParticipantId == marketParticipant.MarketParticipantId)
                .Where(cm => cm.MessageDateTime >= searchCriteria.FromDateTime &&
                             cm.MessageDateTime < searchCriteria.ToDateTime);
        }

        private static IOrderedQueryable<ChargeMessage> SortChargeMessages(
            IQueryable<ChargeMessage> chargeMessages,
            ChargeMessagesSearchCriteriaV1Dto searchCriteria)
        {
            return searchCriteria.ChargeMessageSortColumnName switch
            {
                ChargeMessageSortColumnName.MessageId => searchCriteria.IsDescending
                    ? chargeMessages.OrderByDescending(cm => cm.MessageId)
                    : chargeMessages.OrderBy(cm => cm.MessageId),
                ChargeMessageSortColumnName.MessageType => searchCriteria.IsDescending
                    ? chargeMessages.OrderByDescending(cm => cm.MessageType)
                    : chargeMessages.OrderBy(cm => cm.MessageType),
                ChargeMessageSortColumnName.MessageDateTime => searchCriteria.IsDescending
                    ? chargeMessages.OrderByDescending(cm => cm.MessageDateTime)
                    : chargeMessages.OrderBy(cm => cm.MessageDateTime),
                _ => chargeMessages.OrderBy(cm => cm.MessageDateTime),
            };
        }

        private static ChargeMessageType MapBusinessReasonCode(BusinessReasonCode businessReasonCode) =>
            businessReasonCode switch
            {
                BusinessReasonCode.UpdateChargeInformation => ChargeMessageType.D18,
                BusinessReasonCode.UpdateChargePrices => ChargeMessageType.D08,
                _ => throw new NotSupportedException(
                    $"BusinessReasonCode '{nameof(businessReasonCode)}' is not supported"),
            };
    }
}
