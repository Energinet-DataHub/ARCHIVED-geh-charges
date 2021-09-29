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
            List<ChargeLinkOperation> operations,
            List<ChargeLinkPeriodDetails> periodDetails)
        {
            Id = Guid.NewGuid();
            ChargeId = chargeId;
            MeteringPointId = meteringPointId;
            _operations = operations;
            _periodDetails = periodDetails;
        }

        /// <summary>
        /// Used implicitly by persistence.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ChargeLink(Guid chargeId, Guid meteringPointId)
        {
            ChargeId = chargeId;
            MeteringPointId = meteringPointId;
            _operations = new List<ChargeLinkOperation>();
            _periodDetails = new List<ChargeLinkPeriodDetails>();
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

        private readonly List<ChargeLinkOperation> _operations;

        public IReadOnlyCollection<ChargeLinkOperation> Operations => _operations.AsReadOnly();

        private readonly List<ChargeLinkPeriodDetails> _periodDetails;

        public IReadOnlyCollection<ChargeLinkPeriodDetails> PeriodDetails => _periodDetails.AsReadOnly();
    }
}
