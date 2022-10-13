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
using Energinet.Charges.Contracts.ChargePoint;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using GreenEnergyHub.Iso8601;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices;

public class ChargePointQueryService : IChargePointQueryService
{
    private readonly IData _data;
    private readonly IIso8601Durations _iso8601Durations;

    public ChargePointQueryService(IData data, IIso8601Durations iso8601Durations)
    {
        _data = data;
        _iso8601Durations = iso8601Durations;
    }

    public IList<ChargePointV1Dto> Search(ChargePointSearchCriteriaV1Dto chargePointSearchCriteria)
    {
        var chargePoints = _data.ChargePoints
            .Where(cp => cp.ChargeId == chargePointSearchCriteria.ChargeId)
            .Where(c => c.Time >= chargePointSearchCriteria.DateTimeFrom && c.Time <= chargePointSearchCriteria.DateTimeTo);

        return chargePoints
            .AsChargePointV1Dto(_iso8601Durations);
    }
}
