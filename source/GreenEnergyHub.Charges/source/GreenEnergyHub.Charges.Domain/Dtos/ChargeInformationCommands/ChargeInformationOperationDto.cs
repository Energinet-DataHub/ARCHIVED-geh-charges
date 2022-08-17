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
    public class ChargeInformationOperationDto : OperationBase
    {
        public ChargeInformationOperationDto(
                string operationId,
                ChargeType chargeType,
                string senderProvidedChargeId,
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
            : base(operationId, chargeType, senderProvidedChargeId, chargeOwner, startDateTime, endDateTime)
        {
            Points = new List<Point>();
            ChargeName = chargeName;
            ChargeDescription = chargeDescription;
            Resolution = resolution;
            PriceResolution = priceResolution;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            VatClassification = vatClassification;
            PointsStartInterval = pointsStartInterval;
            PointsEndInterval = pointsEndInterval;
            Points = points;
        }

        /// <summary>
        /// The charge name
        /// </summary>
        public string ChargeName { get; }

        public string ChargeDescription { get; }

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

        public Resolution Resolution { get; }

        public Resolution PriceResolution { get; }

        public Instant? PointsStartInterval { get; }

        public Instant? PointsEndInterval { get; }

        public List<Point> Points { get; }
    }
}
