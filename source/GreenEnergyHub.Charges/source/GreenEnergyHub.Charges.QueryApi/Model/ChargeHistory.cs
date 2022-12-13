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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("ChargeHistory", Schema = "ChargesQuery")]
    [Index("SenderProvidedChargeId", "Type", "Owner", Name = "I1_SenderProvidedChargeId_Type_Owner")]
    public class ChargeHistory
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(35)]
        public string Owner { get; set; }

        [Required]
        [StringLength(132)]
        public string Name { get; set; }

        [Required]
        [StringLength(2048)]
        public string Description { get; set; }

        public int Resolution { get; set; }

        public bool TaxIndicator { get; set; }

        public bool TransparentInvoicing { get; set; }

        public int VatClassification { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime AcceptedDateTime { get; set; }
    }
}
