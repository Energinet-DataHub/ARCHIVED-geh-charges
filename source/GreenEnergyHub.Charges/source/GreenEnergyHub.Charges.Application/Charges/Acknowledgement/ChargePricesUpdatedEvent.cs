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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargePricesUpdatedEvent : MessageBase
    {
        public ChargePricesUpdatedEvent(
            string chargeId,
            ChargeType chargeType,
            string chargeOwner,
            Instant updatePeriodStartDate,
            Instant updatePeriodEndDate,
            List<Point> points)
        {
            ChargeId = chargeId;
            ChargeType = chargeType;
            ChargeOwner = chargeOwner;
            UpdatePeriodStartDate = updatePeriodStartDate;
            UpdatePeriodEndDate = updatePeriodEndDate;
            Points = points;
        }

        public string ChargeId { get; }

        public ChargeType ChargeType { get; }

        public string ChargeOwner { get; }

        public Instant UpdatePeriodStartDate { get; }

        public Instant UpdatePeriodEndDate { get; }

        public List<Point> Points { get; }
    }
}
