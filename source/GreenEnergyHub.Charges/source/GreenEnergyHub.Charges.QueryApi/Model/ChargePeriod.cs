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
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("ChargePeriod", Schema = "Charges")]
    public partial class ChargePeriod
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChargeId { get; set; }

        public bool TransparentInvoicing { get; set; }

        [Required]
        [StringLength(2048)]
        public string Description { get; set; }

        [Required]
        [StringLength(132)]
        public string Name { get; set; }

        public int VatClassification { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime ReceivedDateTime { get; set; }

        public int ReceivedOrder { get; set; }

        [ForeignKey(nameof(ChargeId))]
        [InverseProperty("ChargePeriods")]
        public virtual Charge Charge { get; set; }
    }
}
