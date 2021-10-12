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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class Charge
    {
        public Charge(
            Guid id,
            Document document,
            string chargeOperationId,
            string senderProvidedChargeId,
            string name,
            string description,
            string owner,
            string correlationId,
            Instant startDateTime,
            Instant? endDateTime,
            ChargeType type,
            VatClassification vatClassification,
            Resolution resolution,
            bool transparentInvoicing,
            bool taxIndicator,
            List<Point> points)
        {
            Id = id;
            Document = document;
            ChargeOperationId = chargeOperationId;
            SenderProvidedChargeId = senderProvidedChargeId;
            Name = name;
            Description = description;
            Owner = owner;
            CorrelationId = correlationId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime.TimeOrEndDefault();
            Type = type;
            VatClassification = vatClassification;
            Resolution = resolution;
            TransparentInvoicing = transparentInvoicing;
            TaxIndicator = taxIndicator;
            Points = points;
        }

        /// <summary>
        /// Globally unique identifier of the charge.
        /// </summary>
        public Guid Id { get; }

        public Document Document { get; }

        /// <summary>
        /// Contains a unique ID for the specific Charge Event, provided by the sender.
        /// </summary>
        public string ChargeOperationId { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants and charge type).
        /// Example: EA-001
        /// </summary>
        public string SenderProvidedChargeId { get; }

        public ChargeType Type { get; }

        /// <summary>
        /// The charge name
        /// Example: "Elafgift"
        /// </summary>
        public string Name { get; }

        public string Description { get; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant EndDateTime { get; }

        public VatClassification VatClassification { get; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - private setter used in unit test
        public bool TaxIndicator { get; private set; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string Owner { get; }

        public Resolution Resolution { get; }

        public List<Point> Points { get; }

        public string CorrelationId { get; }
    }
}
