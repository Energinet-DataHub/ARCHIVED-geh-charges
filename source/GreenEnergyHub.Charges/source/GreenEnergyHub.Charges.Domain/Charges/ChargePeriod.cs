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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    /// <summary>
    /// Class is used for handling charge periods and corresponding charge master data in that period.
    /// </summary>
    public class ChargePeriod
    {
        public ChargePeriod(
            Guid id,
            string name,
            string description,
            VatClassification vatClassification,
            bool transparentInvoicing,
            Instant startDateTime,
            Instant endDateTime)
        {
            Id = id;
            Name = name;
            Description = description;
            VatClassification = vatClassification;
            TransparentInvoicing = transparentInvoicing;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        /// <summary>
        /// Globally unique identifier of the charge period.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The charge name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The charge description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// VAT classification for charge
        /// </summary>
        public VatClassification VatClassification { get; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant EndDateTime { get; }
    }
}
