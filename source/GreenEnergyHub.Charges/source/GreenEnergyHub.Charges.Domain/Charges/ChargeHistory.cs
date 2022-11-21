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
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    /// <summary>
    /// Class is used for handling history related to a chargeinformation.
    /// </summary>
    public class ChargeHistory
    {
        private ChargeHistory(
            string senderProvidedChargeId,
            ChargeType type,
            string owner,
            string name,
            string description,
            Resolution resolution,
            bool taxIndicator,
            bool transparentInvoicing,
            VatClassification vatClassification,
            Instant startDateTime,
            Instant? endDateTime,
            Instant acceptedDateTime)
        {
            Id = Guid.NewGuid();
            SenderProvidedChargeId = senderProvidedChargeId;
            Type = type;
            Owner = owner;
            Name = name;
            Description = description;
            Resolution = resolution;
            TaxIndicator = taxIndicator;
            TransparentInvoicing = transparentInvoicing;
            VatClassification = vatClassification;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            AcceptedDateTime = acceptedDateTime;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private ChargeHistory()
        {
            SenderProvidedChargeId = string.Empty;
            Owner = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
        }

        /// <summary>
        /// Globally unique identifier of the charge history reference.
        /// </summary>
        public Guid Id { get; }

        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; }

        public ChargeType Type { get; }

        [Required]
        [StringLength(35)]
        public string Owner { get; }

        [Required]
        [StringLength(132)]
        public string Name { get; }

        [Required]
        [StringLength(2048)]
        public string Description { get; }

        [Required]
        public Resolution Resolution { get; }

        [Required]
        public bool TaxIndicator { get; }

        [Required]
        public bool TransparentInvoicing { get; }

        [Required]
        public VatClassification VatClassification { get; }

        [Required]
        public Instant StartDateTime { get; }

        public Instant? EndDateTime { get; }

        [Required]
        public Instant AcceptedDateTime { get; }

        public static ChargeHistory Create(
            string senderProvidedChargeId,
            ChargeType chargeType,
            string owner,
            string name,
            string description,
            Resolution resolution,
            TaxIndicator taxIndicator,
            TransparentInvoicing transparentInvoicing,
            VatClassification vatClassification,
            Instant startDateTime,
            Instant? endDateTime,
            Instant acceptedDateTime)
        {
            ArgumentNullException.ThrowIfNull(senderProvidedChargeId);
            ArgumentNullException.ThrowIfNull(chargeType);
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(resolution);
            ArgumentNullException.ThrowIfNull(taxIndicator);
            ArgumentNullException.ThrowIfNull(transparentInvoicing);
            ArgumentNullException.ThrowIfNull(vatClassification);
            ArgumentNullException.ThrowIfNull(startDateTime);
            ArgumentNullException.ThrowIfNull(acceptedDateTime);

            return new ChargeHistory(
                senderProvidedChargeId,
                chargeType,
                owner,
                name,
                description,
                resolution,
                ParseTaxIndicator(taxIndicator),
                ParseTransparentInvoicing(transparentInvoicing),
                vatClassification,
                startDateTime,
                endDateTime,
                acceptedDateTime);
        }

        private static bool ParseTaxIndicator(TaxIndicator taxIndicator)
        {
            return taxIndicator switch
            {
                Charges.TaxIndicator.Tax => true,
                Charges.TaxIndicator.NoTax => false,
                _ => throw new InvalidOperationException("Tax indicator must be set."),
            };
        }

        private static bool ParseTransparentInvoicing(TransparentInvoicing transparentInvoicing)
        {
            return transparentInvoicing switch
            {
                Charges.TransparentInvoicing.Transparent => true,
                Charges.TransparentInvoicing.NonTransparent => false,
                _ => throw new InvalidOperationException("Transparent invoicing must be set."),
            };
        }
    }
}
