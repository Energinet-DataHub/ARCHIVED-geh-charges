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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkTransmissionRequest
{
    public class ChargeLinkTransmissionRequest
    {
        public ChargeLinkTransmissionRequest(
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            string chargeId,
            string chargeOwner,
            ChargeType chargeType,
            string meteringPointId,
            int factor,
            Instant startDateTime,
            Instant endDateTime,
            Guid messageHubId)
        {
            Id = Guid.NewGuid();
            RecipientId = recipientId;
            RecipientRole = recipientRole;
            BusinessReasonCode = businessReasonCode;
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            ChargeType = chargeType;
            MeteringPointId = meteringPointId;
            Factor = factor;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            MessageHubId = messageHubId;
        }

        public Guid Id { get; set; }

        public string RecipientId { get; }

        public MarketParticipantRole RecipientRole { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public string MeteringPointId { get; }

        public string ChargeId { get; }

        public string ChargeOwner { get; }

        public ChargeType ChargeType { get; }

        public int Factor { get; }

        public Instant StartDateTime { get; }

        public Instant EndDateTime { get; }

        public Guid MessageHubId { get; }
    }
}
