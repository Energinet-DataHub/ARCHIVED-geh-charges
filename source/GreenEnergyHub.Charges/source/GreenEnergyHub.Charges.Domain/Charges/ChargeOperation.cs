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
using NodaTime;

#pragma warning disable 8618

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
        public ChargeOperation()
        {
            Points = new List<Point>();
        }

        /// <summary>
        /// Contains a unique ID for the specific Charge Event, provided by the sender.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string ChargeId { get; set; }

        public ChargeType Type { get; set; }

        /// <summary>
        /// The charge name
        /// Example: "Elafgift"
        /// </summary>
        public string ChargeName { get; set; }

        public string ChargeDescription { get; set; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; set; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant EndDateTime { get; set; }

        public VatClassification VatClassification { get; set; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; set; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        public bool TaxIndicator { get; set; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string ChargeOwner { get; set; }

        public Resolution Resolution { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; set; }
    }
}
