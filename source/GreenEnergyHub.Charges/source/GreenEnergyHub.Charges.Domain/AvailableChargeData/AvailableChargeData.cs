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
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeData
{
    public class AvailableChargeData : AvailableDataBase
    {
        public AvailableChargeData(
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            string chargeId,
            string chargeOwner,
            ChargeType chargeType,
            string chargeName,
            string chargeDescription,
            Instant startDateTime,
            Instant endDateTime,
            VatClassification vatClassification,
            bool taxIndicator,
            bool transparentInvoicing,
            Resolution resolution,
            List<AvailableChargeDataPoint> points)
            : base(recipientId, recipientRole, businessReasonCode, requestDateTime, availableDataReferenceId)
        {
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            ChargeType = chargeType;
            ChargeName = chargeName;
            ChargeDescription = chargeDescription;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            VatClassification = vatClassification;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            Resolution = resolution;
            _points = points;
        }

        /// <summary>
        /// Used implicitly by persistence.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private AvailableChargeData(string recipientId, string chargeId, string chargeOwner, string chargeName, string chargeDescription)
            : base(recipientId)
        {
            ChargeId = chargeId;
            ChargeOwner = chargeOwner;
            ChargeName = chargeName;
            ChargeDescription = chargeDescription;
            _points = new List<AvailableChargeDataPoint>();
        }

        public string ChargeId { get; }

        public string ChargeOwner { get; }

        public ChargeType ChargeType { get; }

        public string ChargeName { get; }

        public string ChargeDescription { get; }

        public Instant StartDateTime { get; }

        public Instant EndDateTime { get; }

        public VatClassification VatClassification { get; }

        public bool TaxIndicator { get; }

        public bool TransparentInvoicing { get; }

        public Resolution Resolution { get; }

        private readonly List<AvailableChargeDataPoint> _points;

        public IReadOnlyCollection<AvailableChargeDataPoint> Points => _points.AsReadOnly();
    }
}
