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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeEvent integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// The ChargePriceDto class contains the intend of the charge command, e.g. updating an existing charge.
    /// </summary>
    public class ChargePriceDto : ChargeOperation
    {
        public ChargePriceDto(
                string id,
                ChargeType type,
                string chargeId,
                string chargeOwner,
                Instant startDateTime,
                Instant? endDateTime,
                Instant? pointsStartInterval,
                Instant? pointsEndInterval,
                List<Point> points)
            : base(id, chargeId, type, chargeOwner, startDateTime)
        {
            Points = new List<Point>();
            EndDateTime = endDateTime;
            PointsStartInterval = pointsStartInterval;
            PointsEndInterval = pointsEndInterval;
            Points = points;
        }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant? EndDateTime { get; }

        public Instant? PointsStartInterval { get; }

        public Instant? PointsEndInterval { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; }
    }
}
