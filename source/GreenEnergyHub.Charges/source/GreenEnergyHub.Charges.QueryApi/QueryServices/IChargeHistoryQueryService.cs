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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeHistory;

namespace GreenEnergyHub.Charges.QueryApi.QueryServices;

/// <summary>
/// Service for querying charge history
/// </summary>
public interface IChargeHistoryQueryService
{
    /// <summary>
    /// Get charge history for a single charge
    /// </summary>
    /// <returns>A <see cref="ChargeHistoryV1Dto"/></returns>
    Task<IList<ChargeHistoryV1Dto>> SearchAsync(ChargeHistorySearchCriteriaV1Dto searchCriteria);
}