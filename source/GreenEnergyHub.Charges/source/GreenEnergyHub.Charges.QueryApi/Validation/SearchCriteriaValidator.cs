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
using Energinet.Charges.Contracts.Charge;

namespace GreenEnergyHub.Charges.QueryApi.Validation;

public static class SearchCriteriaValidator
{
    /// <summary>
    /// Validate if the search criteria are valid
    /// </summary>
    /// <param name="searchCriteriaDto"></param>
    /// <returns>bool</returns>
    public static bool Validate(SearchCriteriaDto searchCriteriaDto)
    {
        if (!IsOwnerIdValid(searchCriteriaDto))
            return false;

        if (!IsChargeTypeValid(searchCriteriaDto))
            return false;

        return true;
    }

    private static bool IsChargeTypeValid(SearchCriteriaDto searchCriteriaDto)
    {
        if (string.IsNullOrEmpty(searchCriteriaDto.ChargeTypes)) return true;

        var chargeTypes = searchCriteriaDto.ChargeTypes.Split(",");
        foreach (var chargeType in chargeTypes)
        {
            if (!Enum.IsDefined(typeof(ChargeType), chargeType))
                return false;
        }

        return true;
    }

    private static bool IsOwnerIdValid(SearchCriteriaDto searchCriteriaDto)
    {
        if (string.IsNullOrEmpty(searchCriteriaDto.OwnerId)) return true;
        return Guid.TryParse(searchCriteriaDto.OwnerId, out _);
    }
}
