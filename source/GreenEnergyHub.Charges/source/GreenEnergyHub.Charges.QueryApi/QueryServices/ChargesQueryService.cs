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
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices
{
    public class ChargesQueryService : IChargesQueryService
    {
        private readonly IData _data;

        public ChargesQueryService(IData data)
        {
            _data = data;
        }

        public async Task<IList<ChargeV1Dto>> SearchAsync(ChargeSearchCriteriaV1Dto searchCriteria)
        {
            var charges = _data.Charges;

            if (!string.IsNullOrWhiteSpace(searchCriteria.ChargeIdOrName))
                charges = SearchByChargeIdOrName(searchCriteria, charges);

            if (searchCriteria.OwnerIds != null && searchCriteria.OwnerIds.Any())
                charges = SearchByOwnerId(searchCriteria.OwnerIds, charges);

            if (searchCriteria.ChargeTypes != null && searchCriteria.ChargeTypes.Any())
                charges = SearchByChargeTypes(searchCriteria.ChargeTypes, charges);

            var chargeV1Dtos = await charges.AsChargeV1Dto().ToListAsync().ConfigureAwait(false);
            var sortedChargeV1Dtos = SortCharges(chargeV1Dtos);
            return sortedChargeV1Dtos;
        }

        public async Task<IList<ChargeV1Dto>> GetAsOfAsync(Guid chargeId, DateTimeOffset asOf)
        {
            var chargePeriods = _data.ChargePeriods.TemporalAsOf(asOf.UtcDateTime);

            var chargeV1Dtos = await chargePeriods
                .Where(cp => cp.ChargeId == chargeId)
                .Select(x => x.Charge)
                .AsChargeV1Dto()
                .ToListAsync()
                .ConfigureAwait(false);
            var sortedChargeV1Dtos = SortCharges(chargeV1Dtos);
            return sortedChargeV1Dtos;
        }

        private static IList<ChargeV1Dto> SortCharges(IEnumerable<ChargeV1Dto> chargeV1Dtos)
        {
            return chargeV1Dtos
                .OrderBy(c => c.ChargeId)
                .ThenByDescending(c => c.ValidFromDateTime)
                .ToList();
        }

        private static IQueryable<Charge> SearchByChargeIdOrName(
            ChargeSearchCriteriaV1Dto chargeSearchCriteria, IQueryable<Charge> charges)
        {
            charges = charges
                .Where(c => c.SenderProvidedChargeId.Contains(chargeSearchCriteria.ChargeIdOrName)
                            || c.ChargePeriods.Any(cp => cp.Name.Contains(chargeSearchCriteria.ChargeIdOrName)));
            return charges;
        }

        private static IQueryable<Charge> SearchByOwnerId(ICollection<Guid> ownerIds, IQueryable<Charge> charges)
        {
            return charges.Where(c => ownerIds.Contains(c.OwnerId));
        }

        private static IQueryable<Charge> SearchByChargeTypes(ICollection<ChargeType> chargeTypes, IQueryable<Charge> charges)
        {
            charges = charges.Where(c => chargeTypes.Contains((ChargeType)c.Type));
            return charges;
        }
    }
}
