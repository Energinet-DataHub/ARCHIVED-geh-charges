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

using System.Collections.Generic;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeEvent integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// The ChargeEvent class contains the intend of the charge command, e.g. it's an update of a charge plus an ID provided by the sender.
    /// </summary>
    public class ChargeOperation
    {
        public ChargeOperation(
            string chargeOperationId,
            ChargeType chargeType,
            string senderProvidedChargeId,
            string chargeName,
            string description,
            string chargeOwner,
            Resolution chargeResolution,
            bool taxIndicator,
            bool transparentInvoicing,
            VatClassification vatClassification,
            Instant startDateTime,
            Instant? endDateTime,
            List<Point> points)
        {
            Id = chargeOperationId;
            Type = chargeType;
            ChargeId = senderProvidedChargeId;
            ChargeName = chargeName;
            ChargeDescription = description;
            ChargeOwner = chargeOwner;
            Resolution = chargeResolution;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            VatClassification = vatClassification;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Points = points;
        }

        /// <summary>
        /// Contains a unique ID for the specific Charge Event, provided by the sender.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string ChargeId { get; init; }

        public ChargeType Type { get; init; }

        /// <summary>
        /// The charge name
        /// Example: "Elafgift"
        /// </summary>
        public string ChargeName { get; init; }

        public string ChargeDescription { get; init; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; init; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant? EndDateTime { get; init; }

        public VatClassification VatClassification { get; init; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; init; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        public bool TaxIndicator { get; init; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string ChargeOwner { get; init; }

        public Resolution Resolution { get; init; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; init; }
    }
}
