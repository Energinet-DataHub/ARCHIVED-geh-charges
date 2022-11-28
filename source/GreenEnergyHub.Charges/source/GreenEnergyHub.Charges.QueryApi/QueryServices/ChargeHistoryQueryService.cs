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
using Energinet.DataHub.Charges.Contracts.ChargeHistory;
using GreenEnergyHub.Charges.QueryApi.Model;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices
{
    public class ChargeHistoryQueryService : IChargeHistoryQueryService
    {
        private readonly IData _data;

        public ChargeHistoryQueryService(IData data)
        {
            _data = data;
        }

        public async Task<IList<ChargeHistoryV1Dto>> GetAsync(ChargeHistorySearchCriteriaV1Dto searchCriteria)
        {
            var chargeHistories = await _data.ChargeHistories
                .Where(c => c.SenderProvidedChargeId == searchCriteria.ChargeId
                            && (ChargeType)c.Type == searchCriteria.ChargeType
                            && c.Owner == searchCriteria.ChargeOwner
                            && c.AcceptedDateTime <= searchCriteria.AtDateTimeUtc)
                .ToListAsync().ConfigureAwait(false);

            chargeHistories = chargeHistories
                    .GroupBy(c => c.StartDateTime)
                    .Select(c => c.MaxBy(d => d.AcceptedDateTime))
                    .OrderBy(c => c!.StartDateTime)
                    .ToList();

            return MapToChargeHistoryV1Dtos(chargeHistories);
        }

        private static IList<ChargeHistoryV1Dto> MapToChargeHistoryV1Dtos(IList<ChargeHistory> chargeHistories)
        {
            var lastIndex = chargeHistories.IndexOf(chargeHistories.Last());

            return chargeHistories
                .Select((c, i) => new ChargeHistoryV1Dto(
                    DateTime.SpecifyKind(c.StartDateTime, DateTimeKind.Utc),
                    i == lastIndex ? null : DateTime.SpecifyKind(chargeHistories[i + 1].StartDateTime, DateTimeKind.Utc),
                    c.Name,
                    c.Description,
                    (Resolution)c.Resolution,
                    (VatClassification)c.VatClassification,
                    c.TaxIndicator,
                    c.TransparentInvoicing,
                    (ChargeType)c.Type,
                    c.Owner))
                .ToList();
        }
    }
}
