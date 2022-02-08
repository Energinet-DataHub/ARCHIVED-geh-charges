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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("MeteringPoint", Schema = "Charges")]
    [Index(nameof(GridAreaLinkId), Name = "IX_GridAreaLinkId")]
    [Index(nameof(MeteringPointId), Name = "IX_MeteringPointId")]
    [Index(nameof(MeteringPointId), Name = "UC_MeteringPointId", IsUnique = true)]
    public partial class MeteringPoint
    {
        public MeteringPoint()
        {
            ChargeLinks = new HashSet<ChargeLink>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MeteringPointId { get; set; }

        public int MeteringPointType { get; set; }

        public Guid GridAreaLinkId { get; set; }

        public DateTime EffectiveDate { get; set; }

        public int ConnectionState { get; set; }

        public int? SettlementMethod { get; set; }

        [ForeignKey(nameof(GridAreaLinkId))]
        [InverseProperty("MeteringPoints")]
        public virtual GridAreaLink GridAreaLink { get; set; }

        [InverseProperty(nameof(ChargeLink.MeteringPoint))]
        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }
    }
}
