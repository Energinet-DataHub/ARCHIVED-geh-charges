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

using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeEvent integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// The ChargeOperationDto class contains the intend of the charge command, e.g. updating an existing charge.
    /// </summary>
    public class ChargeOperationDto : ChargeOperationBase
    {
        public ChargeOperationDto(
                string id,
                ChargeType type,
                string chargeId,
                string chargeName,
                string chargeDescription,
                string chargeOwner,
                Resolution resolution,
                Resolution priceResolution,
                TaxIndicator taxIndicator,
                TransparentInvoicing transparentInvoicing,
                VatClassification vatClassification,
                Instant startDateTime,
                Instant? endDateTime,
                Instant? pointsStartInterval,
                Instant? pointsEndInterval,
                List<Point> points)
        {
            Points = new List<Point>();
            Id = id;
            Type = type;
            ChargeId = chargeId;
            ChargeName = chargeName;
            ChargeDescription = chargeDescription;
            ChargeOwner = chargeOwner;
            Resolution = resolution;
            PriceResolution = priceResolution;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            VatClassification = vatClassification;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            PointsStartInterval = pointsStartInterval;
            PointsEndInterval = pointsEndInterval;
            Points = points;
        }

        /// <summary>
        /// Contains a unique ID for the specific Charge OperationId, provided by the sender.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string ChargeId { get; }

        public ChargeType Type { get; }

        /// <summary>
        /// The charge name
        /// </summary>
        public string ChargeName { get; }

        public string ChargeDescription { get; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant? EndDateTime { get; }

        public VatClassification VatClassification { get; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This enum can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public TransparentInvoicing TransparentInvoicing { get; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        public TaxIndicator TaxIndicator { get; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string ChargeOwner { get; }

        public Resolution Resolution { get; }

        public Resolution PriceResolution { get; }

        public Instant? PointsStartInterval { get; }

        public Instant? PointsEndInterval { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; }
    }
}
