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
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargePoint;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Iso8601;
using NodaTime.Extensions;

namespace GreenEnergyHub.Charges.QueryApi.ModelPredicates;

public static class ChargePointQueryLogic
{
    public static IQueryable<ChargePointV1Dto> AsChargePointV1Dto(
        this IQueryable<ChargePoint> queryable,
        IIso8601Durations iso8601Durations)
    {
        return queryable.Select(cp => new ChargePointV1Dto(
            cp.Price,
            cp.Time,
            iso8601Durations.GetTimeFixedToDuration(
                    DateTime.SpecifyKind(cp.Time, DateTimeKind.Utc).ToInstant(),
                    ((Resolution)cp.Charge.Resolution).ToString(),
                    1)
                .ToDateTimeOffset()));
    }
}
