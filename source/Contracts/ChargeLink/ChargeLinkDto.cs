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
using Energinet.Charges.Contracts.Charge;

// ReSharper disable once CheckNamespace - Type is shared so namespace is not determined by project structure/namespace
namespace Energinet.Charges.Contracts.ChargeLink
{
    /// <summary>
    /// Represents a Charge Link
    /// </summary>
    public class ChargeLinkDto
    {
        /// <summary>
        /// The type of charge; tariff, fee or subscription
        /// </summary>
        [Required]
        public ChargeType ChargeType { get; set; }

        /// <summary>
        /// A charge identifier provided by the market participant. Combined with charge owner and charge type it becomes unique
        /// </summary>
        [Required]
        public string ChargeId { get; set; }

        /// <summary>
        /// Charge name provided by the market participant.
        /// </summary>
        [Required]
        public string ChargeName { get; set; }

        /// <summary>
        /// A charge owner identification, e.g. the market participant's GLN or EIC number
        /// </summary>
        [Required]
        public string ChargeOwner { get; set; }

        /// <summary>
        /// The market participant's company name
        /// </summary>
        [Required]
        public string ChargeOwnerName { get; set; }

        /// <summary>
        /// Indicates whether a tariff is considered a tax or not
        /// </summary>
        [Required]
        public bool TaxIndicator { get; set; }

        /// <summary>
        /// Indicates whether the charge owner wants the charge to be displayed on the customer invoice
        /// </summary>
        [Required]
        public bool TransparentInvoicing { get; set; }

        /// <summary>
        /// The charge link's quantity
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// The charge link's start date time in UTC
        /// </summary>
        [Required]
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// The charge link's end date time in UTC
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }
    }
}
