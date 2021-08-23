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

using GreenEnergyHub.Charges.Domain.Messages.Events;
using GreenEnergyHub.Charges.Domain.MeteringPoint;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Integration
{
    public class CreateLinkCommandEvent : InboundIntegrationEvent
    {
        /// <summary>
        /// Event raised by the MeteringPoint domain when the charge domain
        /// is asked to add links to a metering point
        /// </summary>
        /// <param name="meteringPointId">The metering point to add links to</param>
        /// <param name="meteringPointType">The type of the metering point, which will influence which links are added</param>
        /// <param name="startDateTime">The date from which links should be added</param>
        public CreateLinkCommandEvent(
            string meteringPointId,
            MeteringPointType meteringPointType,
            Instant startDateTime)
            : base(Transaction.NewTransaction())
        {
            MeteringPointId = meteringPointId;
            MeteringPointType = meteringPointType;
            StartDateTime = startDateTime;
        }

        /// <summary>
        /// Metering point ID to add links to
        /// </summary>
        public string MeteringPointId { get; }

        /// <summary>
        /// Type of the metering point to add links to
        /// </summary>
        public MeteringPointType MeteringPointType { get; }

        /// <summary>
        /// The date from which links should be added
        /// </summary>
        public Instant StartDateTime { get; }
    }
}
