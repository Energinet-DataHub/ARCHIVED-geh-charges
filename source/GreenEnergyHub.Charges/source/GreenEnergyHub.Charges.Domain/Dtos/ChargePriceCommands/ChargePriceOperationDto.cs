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
    /// <summary>
    /// The ChargePriceOperationDto class contains the price series for a charge.
    /// </summary>
    public class ChargePriceOperationDto : ChargeOperationDto
    {
        public ChargePriceOperationDto(
            string operationId,
            ChargeType chargeType,
            string senderProvidedChargeId,
            string chargeOwner,
            Instant startDateTime,
            Instant? endDateTime,
            Instant pointsStartInterval,
            Instant pointsEndInterval,
            Resolution resolution,
            List<Point> points)
            : base(operationId, chargeType, senderProvidedChargeId, chargeOwner, startDateTime, endDateTime)
        {
            Resolution = resolution;
            PointsStartInterval = pointsStartInterval;
            PointsEndInterval = pointsEndInterval;
            Points = points;
        }

        public Resolution Resolution { get; }

        public Instant PointsStartInterval { get; }

        public Instant PointsEndInterval { get; }

        public List<Point> Points { get; }
    }
}
