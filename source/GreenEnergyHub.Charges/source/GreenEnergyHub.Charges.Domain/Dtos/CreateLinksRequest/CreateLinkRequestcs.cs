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

using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Messaging.MessageTypes.Common;

namespace GreenEnergyHub.Charges.Domain.Dtos.CreateLinkRequest
{
    public class CreateLinksCommandEvent : InboundIntegrationEvent
    {
        /// <summary>
        /// Event raised by the MeteringPoint domain when the charge domain
        /// is asked to add links to a metering point
        /// </summary>
        /// <param name="meteringPointId">The metering point to add links to</param>
        public CreateLinksCommandEvent(
            string meteringPointId)
            : base(Transaction.NewTransaction())
        {
            MeteringPointId = meteringPointId;
        }

        /// <summary>
        /// Metering point ID to add links to
        /// </summary>
        public string MeteringPointId { get; }
    }
}
