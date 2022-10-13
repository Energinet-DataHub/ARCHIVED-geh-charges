﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.Charges.Contracts.Charge;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices;

public class ChargesQueryService : IChargesQueryService
{
    private readonly IData _data;

    public ChargesQueryService(IData data)
    {
        _data = data;
    }

    public async Task<IList<ChargeV1Dto>> SearchAsync(ChargeSearchCriteriaV1Dto chargeSearchCriteria)
    {
        var charges = _data.Charges;
        var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();

        charges = ActiveCharges(charges);

        if (!string.IsNullOrWhiteSpace(chargeSearchCriteria.ChargeIdOrName))
            charges = SearchByChargeIdOrName(chargeSearchCriteria, charges, todayAtMidnightUtc);

        if (chargeSearchCriteria.OwnerIds != null && chargeSearchCriteria.OwnerIds.Any())
            charges = SearchByOwnerId(chargeSearchCriteria.OwnerIds, charges);

        if (chargeSearchCriteria.ChargeTypes != null && chargeSearchCriteria.ChargeTypes.Any())
            charges = SearchByChargeTypes(chargeSearchCriteria.ChargeTypes, charges);

        return await charges
            .AsChargeV1Dto()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    private static IQueryable<Charge> SearchByChargeIdOrName(
        ChargeSearchCriteriaV1Dto chargeSearchCriteria, IQueryable<Charge> charges, DateTime todayAtMidnightUtc)
    {
        charges = charges
            .Where(c => c.SenderProvidedChargeId.Contains(chargeSearchCriteria.ChargeIdOrName)
                        || c.ChargePeriods
                            .OrderByDescending(cp => cp.StartDateTime)
                            .First(cp => cp.StartDateTime <= todayAtMidnightUtc)
                            .Name.Contains(chargeSearchCriteria.ChargeIdOrName));
        return charges;
    }

    private static IQueryable<Charge> SearchByOwnerId(List<Guid> ownerIds, IQueryable<Charge> charges)
    {
        return charges.Where(c => ownerIds.Contains(c.OwnerId));
    }

    private static IQueryable<Charge> SearchByChargeTypes(ICollection<ChargeType> chargeTypes, IQueryable<Charge> charges)
    {
        charges = charges.Where(c => chargeTypes.Contains((ChargeType)c.Type));
        return charges;
    }

    private static IQueryable<Charge> ActiveCharges(IQueryable<Charge> charges)
    {
        var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();
        return charges.Where(c => c.ChargePeriods.Any(cp => cp.StartDateTime <= todayAtMidnightUtc));
    }
}
