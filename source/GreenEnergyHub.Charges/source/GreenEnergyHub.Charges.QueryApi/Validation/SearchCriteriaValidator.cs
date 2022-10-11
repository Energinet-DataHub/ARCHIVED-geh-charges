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
using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.QueryApi.Validation;

public static class SearchCriteriaValidator
{
    /// <summary>
    /// Validate if the search criteria are valid
    /// </summary>
    /// <param name="searchCriteriaV1Dto"></param>
    /// <returns>bool</returns>
    public static bool Validate(SearchCriteriaV1Dto searchCriteriaV1Dto)
    {
        if (!IsOwnerIdsValid(searchCriteriaV1Dto.OwnerIds))
            return false;

        if (!IsChargeTypesValid(searchCriteriaV1Dto.ChargeTypes))
            return false;

        return true;
    }

    private static bool IsChargeTypesValid(List<ChargeType> chargeTypes)
    {
        if (chargeTypes == null || !chargeTypes.Any()) return true;

        foreach (var chargeType in chargeTypes)
        {
            if (!Enum.IsDefined(typeof(ChargeType), chargeType))
                return false;
        }

        return true;
    }

    private static bool IsOwnerIdsValid(List<Guid> ownerIds)
    {
        if (ownerIds == null || !ownerIds.Any()) return true;

        return ownerIds.All(id => id != Guid.Empty);
    }
}
