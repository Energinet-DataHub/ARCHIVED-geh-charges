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
using System.Linq;
using Energinet.DataHub.Charges.Contracts.ChargePrice;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using GreenEnergyHub.Iso8601;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices;

public class ChargePriceQueryService : IChargePriceQueryService
{
    private readonly IData _data;
    private readonly IIso8601Durations _iso8601Durations;

    public ChargePriceQueryService(IData data, IIso8601Durations iso8601Durations)
    {
        _data = data;
        _iso8601Durations = iso8601Durations;
    }

    public IList<ChargePriceV1Dto> Search(ChargePricesSearchCriteriaV1Dto searchCriteria)
    {
        var chargePoints = _data.ChargePoints
            .Where(cp => cp.ChargeId == searchCriteria.ChargeId)
            .Where(c => c.Time >= searchCriteria.FromDateTime && c.Time < searchCriteria.ToDateTime);

        chargePoints = SortChargePoints(searchCriteria, chargePoints);

        var chargePrices = chargePoints
            .Skip(searchCriteria.Skip)
            .Take(searchCriteria.Take)
            .AsChargePriceV1Dto(_iso8601Durations);

        return SortChargePrices(searchCriteria, chargePrices);
    }

    private static IList<ChargePriceV1Dto> SortChargePrices(ChargePricesSearchCriteriaV1Dto searchCriteria, IList<ChargePriceV1Dto> chargePoints)
    {
        var sortColumnName = searchCriteria.SortColumnName.ToString();
        return searchCriteria.IsDescending
            ? chargePoints.OrderByDescending(cp => cp.GetType().GetProperty(sortColumnName)?.GetValue(cp)).ToList()
            : chargePoints.OrderBy(cp => cp.GetType().GetProperty(sortColumnName)?.GetValue(cp)).ToList();
    }

    private static IQueryable<ChargePoint> SortChargePoints(
        ChargePricesSearchCriteriaV1Dto searchCriteria,
        IQueryable<ChargePoint> chargePoints)
    {
        return searchCriteria.SortColumnName switch
        {
            SortColumnName.FromDateTime => searchCriteria.IsDescending
                ? chargePoints.OrderByDescending(cp => cp.Time)
                : chargePoints.OrderBy(cp => cp.Time),
            SortColumnName.Price => searchCriteria.IsDescending
                ? chargePoints.OrderByDescending(cp => cp.Price)
                : chargePoints.OrderBy(cp => cp.Price),
            _ => chargePoints,
        };
    }
}
