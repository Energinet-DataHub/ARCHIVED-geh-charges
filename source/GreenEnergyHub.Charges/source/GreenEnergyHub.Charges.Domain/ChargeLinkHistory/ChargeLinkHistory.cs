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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkHistory
{
    public class ChargeLinkHistory
    {
        public ChargeLinkHistory(
            string recipient,
            MarketParticipantRole marketParticipantRole,
            BusinessReasonCode businessReasonCode,
            string chargeId,
            string meteringPointId,
            string owner,
            int factor,
            ChargeType chargeType,
            Instant validFrom,
            Instant validTo,
            Guid postOfficeId)
        {
            Id = Guid.NewGuid();
            Recipient = recipient;
            MarketParticipantRole = marketParticipantRole;
            BusinessReasonCode = businessReasonCode;
            ChargeId = chargeId;
            MeteringPointId = meteringPointId;
            Owner = owner;
            Factor = factor;
            ChargeType = chargeType;
            ValidFrom = validFrom;
            ValidTo = validTo;
            PostOfficeId = postOfficeId;
        }

        public Guid Id { get; set; }

        public string Recipient { get; }

        public MarketParticipantRole MarketParticipantRole { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public string ChargeId { get; }

        public string MeteringPointId { get; }

        public string Owner { get; }

        public int Factor { get; }

        public ChargeType ChargeType { get; }

        public Instant ValidFrom { get; }

        public Instant ValidTo { get; }

        public Guid PostOfficeId { get; }
    }
}
