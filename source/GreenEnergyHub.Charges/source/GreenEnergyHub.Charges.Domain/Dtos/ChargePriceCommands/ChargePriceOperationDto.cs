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
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands
{
    public class ChargePriceOperationDto : ChargeOperationBase
    {
        public ChargePriceOperationDto(
            string id,
            ChargeType type,
            string chargeId,
            string chargeOwner,
            Instant startDate,
            Instant? endDateTime,
            Instant pointsStartInterval,
            Instant pointsEndInterval,
            Resolution resolution,
            List<Point> points)
        {
            Id = id;
            Type = type;
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            Resolution = resolution;
            StartDate = startDate;
            EndDateTime = endDateTime;
            PointsStartInterval = pointsStartInterval;
            PointsEndInterval = pointsEndInterval;
            Points = points;
        }

        public string Id { get; }

        public ChargeType Type { get; }

        public string ChargeId { get; }

        public string ChargeOwner { get; }

        public Resolution Resolution { get; }

        public Instant StartDate { get; }

        public Instant? EndDateTime { get; }

        public Instant PointsStartInterval { get; }

        public Instant PointsEndInterval { get; }

        public List<Point> Points { get; }
    }
}
