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

using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents
{
    public class ChargeLinkCreatedEvent : OutboundIntegrationEvent
    {
        public ChargeLinkCreatedEvent(
            string chargeLinkId,
            string meteringPointId,
            string chargeId,
            ChargeType chargeType,
            string chargeOwner,
            ChargeLinkPeriod chargeLinkPeriod)
        {
            ChargeLinkId = chargeLinkId;
            MeteringPointId = meteringPointId;
            ChargeId = chargeId;
            ChargeType = chargeType;
            ChargeOwner = chargeOwner;
            ChargeLinkPeriod = chargeLinkPeriod;
        }

        public string ChargeLinkId { get; }

        public string MeteringPointId { get; }

        public string ChargeId { get; }

        public ChargeType ChargeType { get; }

        public string ChargeOwner { get; }

        public ChargeLinkPeriod ChargeLinkPeriod { get; }
    }
}
