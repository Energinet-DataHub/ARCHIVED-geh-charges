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
        [Required]
        public ChargeType ChargeType { get; set; }

        [Required]
        public string ChargeId { get; set; }

        [Required]
        public string ChargeName { get; set; }

        [Required]
        public string ChargeOwner { get; set; }

        [Required]
        public string ChargeOwnerName { get; set; }

        [Required]
        public bool TaxIndicator { get; set; }

        [Required]
        public bool TransparentInvoicing { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }
}
