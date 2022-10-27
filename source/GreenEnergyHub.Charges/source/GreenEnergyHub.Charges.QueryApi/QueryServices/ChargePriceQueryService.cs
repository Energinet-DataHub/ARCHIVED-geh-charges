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

    public ChargePricesV1Dto Search(ChargePricesSearchCriteriaV1Dto searchCriteria)
    {
        var chargePoints = _data.ChargePoints
            .Where(cp => cp.ChargeId == searchCriteria.ChargeId)
            .Where(c => c.Time >= searchCriteria.FromDateTimeUtc && c.Time <= searchCriteria.ToDateTimeUtc);

        chargePoints = SortChargePoints(searchCriteria, chargePoints);

        var chargePrices = chargePoints
            .Skip(searchCriteria.Skip)
            .Take(searchCriteria.Take)
            .AsChargePriceV1Dto(_iso8601Durations);

        var chargePricesCount = chargePoints.Count();
        return MapToChargePricesV1Dto(SortChargePrices(searchCriteria, chargePrices), chargePricesCount);
    }

    private static ChargePricesV1Dto MapToChargePricesV1Dto(IList<ChargePriceV1Dto> chargePrices, int chargePricesCount)
    {
        return new ChargePricesV1Dto(chargePrices.ToList(), chargePricesCount);
    }

    private static IList<ChargePriceV1Dto> SortChargePrices(ChargePricesSearchCriteriaV1Dto searchCriteria, IList<ChargePriceV1Dto> chargePrices)
    {
        return searchCriteria.ChargePriceSortColumnName switch
        {
            ChargePriceSortColumnName.FromDateTime => searchCriteria.IsDescending
                ? chargePrices.OrderByDescending(cp => cp.FromDateTime).ToList()
                : chargePrices.OrderBy(cp => cp.FromDateTime).ToList(),
            ChargePriceSortColumnName.Price => searchCriteria.IsDescending
                ? chargePrices.OrderByDescending(cp => cp.Price).ToList()
                : chargePrices.OrderBy(cp => cp.Price).ToList(),
            _ => chargePrices,
        };
    }

    private static IQueryable<ChargePoint> SortChargePoints(
        ChargePricesSearchCriteriaV1Dto searchCriteria,
        IQueryable<ChargePoint> chargePoints)
    {
        return searchCriteria.ChargePriceSortColumnName switch
        {
            ChargePriceSortColumnName.FromDateTime => searchCriteria.IsDescending
                ? chargePoints.OrderByDescending(cp => cp.Time)
                : chargePoints.OrderBy(cp => cp.Time),
            ChargePriceSortColumnName.Price => searchCriteria.IsDescending
                ? chargePoints.OrderByDescending(cp => cp.Price)
                : chargePoints.OrderBy(cp => cp.Price),
            _ => chargePoints,
        };
    }
}
