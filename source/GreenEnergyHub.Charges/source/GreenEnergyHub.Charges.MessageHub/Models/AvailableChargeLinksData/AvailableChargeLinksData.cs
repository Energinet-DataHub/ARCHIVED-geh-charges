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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData
{
    public class AvailableChargeLinksData : AvailableDataBase
    {
        public AvailableChargeLinksData(
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            string chargeId,
            string chargeOwner,
            ChargeType chargeType,
            string meteringPointId,
            int factor,
            Instant startDateTime,
            Instant endDateTime,
            DocumentType documentType,
            int operationOrder)
            //Guid actorId)
                : base(
                    senderId,
                    senderRole,
                    recipientId,
                    recipientRole,
                    businessReasonCode,
                    requestDateTime,
                    availableDataReferenceId,
                    documentType,
                    operationOrder)
                    //actorId)
        {
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            ChargeType = chargeType;
            MeteringPointId = meteringPointId;
            Factor = factor;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public string MeteringPointId { get; }

        public string ChargeId { get; }

        /// <summary>
        /// Market Participant Id of Owner.
        /// </summary>
        public string ChargeOwner { get; }

        public ChargeType ChargeType { get; }

        public int Factor { get; }

        public Instant StartDateTime { get; }

        public Instant EndDateTime { get; }
    }
}
