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
using Energinet.DataHub.Charges.Contracts.ChargePoint;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Iso8601;
using NodaTime.Extensions;

namespace GreenEnergyHub.Charges.QueryApi.ModelPredicates;

public static class ChargePointQueryLogic
{
    public static IList<ChargePointV1Dto> AsChargePointV1Dto(
        this IQueryable<ChargePoint> queryable,
        IIso8601Durations iso8601Durations)
    {
        var chargePoints = queryable
            .OrderBy(c => c.Time)
            .Select(cp => new ChargePointV1Dto(
                cp.Price,
                cp.Time,
                iso8601Durations.GetTimeFixedToDuration(
                        DateTime.SpecifyKind(cp.Time, DateTimeKind.Utc).ToInstant(),
                        ((Resolution)cp.Charge.Resolution).ToString(),
                        1)
                    .ToDateTimeUtc()))
            .ToList();

        return chargePoints
            .Select((cp, index) => MapChargePointV1Dto(chargePoints, index, cp))
            .ToList();
    }

    /// <summary>
    /// This mapper ensures that there will be no overlaps between date times.
    /// 'ChargePoints' have to be ordered by time.
    /// </summary>
    /// <param name="chargePoints">test me</param>
    /// <param name="index"></param>
    /// <param name="chargePoint"></param>
    /// <returns>Returns charge point dto with correct 'ActiveToDateTime' date time</returns>
    private static ChargePointV1Dto MapChargePointV1Dto(IList<ChargePointV1Dto> chargePoints, int index, ChargePointV1Dto chargePoint)
    {
        var lastIndex = chargePoints.IndexOf(chargePoints.Last());
        if (index != lastIndex)
        {
            var nextPoint = chargePoints[index + 1];
            var isOverlapping = nextPoint.ActiveFromDateTime < chargePoint.ActiveToDateTime &&
                                nextPoint.ActiveFromDateTime > chargePoint.ActiveFromDateTime;
            if (isOverlapping)
            {
                return new ChargePointV1Dto(
                    chargePoint.Price,
                    chargePoint.ActiveFromDateTime,
                    nextPoint.ActiveFromDateTime);
            }
        }

        return new ChargePointV1Dto(
            chargePoint.Price,
            chargePoint.ActiveFromDateTime,
            chargePoint.ActiveToDateTime);
    }
}
