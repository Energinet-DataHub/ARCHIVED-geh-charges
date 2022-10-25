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
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargePrice;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Iso8601;
using NodaTime.Extensions;

namespace GreenEnergyHub.Charges.QueryApi.ModelPredicates;

public static class ChargePointQueryLogic
{
    public static IList<ChargePriceV1Dto> AsChargePriceV1Dto(
        this IQueryable<ChargePoint> queryable,
        IIso8601Durations iso8601Durations)
    {
        var chargePrices = queryable
            .Select(cp => new ChargePriceV1Dto(
                cp.Price,
                cp.Time,
                iso8601Durations.GetTimeFixedToDuration(
                        DateTime.SpecifyKind(cp.Time, DateTimeKind.Utc).ToInstant(),
                        ((Resolution)cp.Charge.Resolution).ToString(),
                        1)
                    .ToDateTimeUtc()))
            .ToList();

        chargePrices = chargePrices.OrderBy(cp => cp.FromDateTime).ToList();
        return chargePrices
            .Select((cp, index) => MapChargePriceV1Dto(chargePrices, index, cp))
            .ToList();
    }

    /// <summary>
    /// This mapper ensures that there will be no overlaps between date times.
    /// 'ChargePrices' have to be ordered by time.
    /// </summary>
    /// <param name="chargePrices"></param>
    /// <param name="index"></param>
    /// <param name="chargePrice"></param>
    /// <returns>Returns charge price dto with correct 'ToDateTime' date time</returns>
    private static ChargePriceV1Dto MapChargePriceV1Dto(IList<ChargePriceV1Dto> chargePrices, int index, ChargePriceV1Dto chargePrice)
    {
        var lastIndex = chargePrices.IndexOf(chargePrices.Last());
        if (index != lastIndex)
        {
            var nextPoint = chargePrices[index + 1];
            var isOverlapping = nextPoint.FromDateTime < chargePrice.ToDateTime &&
                                nextPoint.FromDateTime > chargePrice.FromDateTime;
            if (isOverlapping)
            {
                return new ChargePriceV1Dto(
                    chargePrice.Price,
                    chargePrice.FromDateTime,
                    nextPoint.FromDateTime);
            }
        }

        return new ChargePriceV1Dto(
            chargePrice.Price,
            chargePrice.FromDateTime,
            chargePrice.ToDateTime);
    }
}
