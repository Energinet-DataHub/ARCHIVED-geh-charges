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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData
{
    public class AvailableChargePriceData : AvailableDataBase
    {
        public AvailableChargePriceData(
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
            Instant startDateTime,
            Resolution resolution,
            DocumentType documentType,
            int operationOrder,
            Guid actorId,
            List<AvailableChargePriceDataPoint> points)
            : base(
                senderId,
                senderRole,
                recipientId,
                recipientRole,
                businessReasonCode,
                requestDateTime,
                availableDataReferenceId,
                documentType,
                operationOrder,
                actorId)
        {
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            ChargeType = chargeType;
            StartDateTime = startDateTime;
            Resolution = resolution;
            _points = points;
        }

        // ReSharper disable once UnusedMember.Local - Used implicitly by persistence
        private AvailableChargePriceData()
        {
            ChargeId = null!;
            ChargeOwner = null!;
            _points = new List<AvailableChargePriceDataPoint>();
        }

        public string ChargeId { get; }

        public string ChargeOwner { get; }

        public ChargeType ChargeType { get; }

        public Instant StartDateTime { get; }

        public Resolution Resolution { get; }

        private readonly List<AvailableChargePriceDataPoint> _points;

        public IReadOnlyCollection<AvailableChargePriceDataPoint> Points => _points.AsReadOnly();
    }
}
