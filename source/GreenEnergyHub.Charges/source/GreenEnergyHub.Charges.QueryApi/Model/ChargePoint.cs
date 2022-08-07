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
    [Table("ChargePoint", Schema = "Charges")]
    public partial class ChargePoint
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChargeId { get; set; }

        public DateTime Time { get; set; }

        [Column(TypeName = "decimal(14, 6)")]
        public decimal Price { get; set; }

        [ForeignKey(nameof(ChargeId))]
        [InverseProperty("ChargePoints")]
        public virtual Charge Charge { get; set; }
    }
}
