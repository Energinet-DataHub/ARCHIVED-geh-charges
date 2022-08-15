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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    /// <summary>
    /// The link between a metering point and a charge.
    /// </summary>
    public class ChargeLink
    {
        public ChargeLink(
            Guid chargeId,
            Guid meteringPointId,
            Instant startDate,
            Instant endDate,
            int factor)
        {
            Id = Guid.NewGuid();
            ChargeId = chargeId;
            MeteringPointId = meteringPointId;
            StartDate = startDate;
            EndDate = endDate;
            Factor = factor;
        }

        /// <summary>
        /// Globally unique identifier of the charge link.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The charge that is linked to the metering point (<see cref="MeteringPointId"/>).
        /// This is not
        /// </summary>
        public Guid ChargeId { get; }

        /// <summary>
        /// The metering point that is linked to the charge (<see cref="ChargeId"/>).
        /// </summary>
        public Guid MeteringPointId { get; }

        public Instant StartDate { get; }

        public Instant EndDate { get; }

        public int Factor { get; }
    }
}
