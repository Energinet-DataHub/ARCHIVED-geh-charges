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
using System.Linq;
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class Charge
    {
        public ChargeType GetChargeType()
        {
            return (ChargeType)Type;
        }

        public Resolution GetResolution()
        {
            return (Resolution)Resolution;
        }

        public string GetChargeName(DateTime todayAtMidnightUtc)
        {
            return GetCurrentOrPlannedChargePeriod(todayAtMidnightUtc).Name;
        }

        public bool IsTransparentInvoicing(DateTime todayAtMidnightUtc)
        {
            return GetCurrentOrPlannedChargePeriod(todayAtMidnightUtc).TransparentInvoicing;
        }

        public DateTime GetValidFromDate(DateTime todayAtMidnightUtc)
        {
            return GetCurrentOrPlannedChargePeriod(todayAtMidnightUtc).StartDateTime;
        }

        public DateTime GetValidToDate()
        {
            return ChargePeriods.MaxBy(cp => cp.EndDateTime)!.EndDateTime;
        }

        private ChargePeriod GetCurrentOrPlannedChargePeriod(DateTime todayAtMidnightUtc)
        {
            return ChargePeriods
                       .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                       .MaxBy(cp => cp.StartDateTime)
                   ??
                   ChargePeriods
                       .OrderBy(cp => cp.StartDateTime)
                       .First();
        }
    }
}
