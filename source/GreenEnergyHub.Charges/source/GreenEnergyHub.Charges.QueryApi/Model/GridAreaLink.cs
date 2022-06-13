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
    [Table("GridAreaLink", Schema = "Charges")]
    [Index(nameof(Id), Name = "IX_GridAreaLinkId")]
    public partial class GridAreaLink
    {
        public GridAreaLink()
        {
            MeteringPoints = new HashSet<MeteringPoint>();
        }

        [Key]
        public Guid Id { get; set; }

        public Guid GridAreaId { get; set; }

        public Guid? OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        [InverseProperty(nameof(MarketParticipant.GridAreaLinks))]
        public virtual MarketParticipant GridAccessProvider { get; set; }

        [InverseProperty(nameof(MeteringPoint.GridAreaLink))]
        public virtual ICollection<MeteringPoint> MeteringPoints { get; set; }
    }
}
