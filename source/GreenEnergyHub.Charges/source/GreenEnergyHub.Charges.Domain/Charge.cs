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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain
{
    public class Charge
    {
#pragma warning disable 8618
        public Charge()
#pragma warning restore 8618
        {
            Points = new List<Point>();
        }

        public Document Document { get; set; }

        /// <summary>
        /// Contains a unique ID for the specific Charge Event, provided by the sender.
        /// </summary>
        public string ChargeOperationId { get; set; }

        public BusinessReasonCode BusinessReasonCode { get; set; }

        public OperationType Status { get; set; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string Id { get; set; }

        public ChargeType Type { get; set; }

        /// <summary>
        /// The charge name
        /// Example: "Elafgift"
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; set; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant? EndDateTime { get; set; }

        public Vat Vat { get; set; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; set; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        public bool Tax { get; set; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string Owner { get; set; }

        public Resolution Resolution { get; set; }

        /// <summary>
        /// PTA: Is this relevant for an incoming charge command?
        /// </summary>
        public string LastUpdatedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; set; }
    }
}
