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
    [Table("MarketParticipant", Schema = "Charges")]
    [Index(nameof(MarketParticipantId), Name = "UC_MarketParticipantId", IsUnique = true)]
    public partial class MarketParticipant
    {
        public MarketParticipant()
        {
            Charges = new HashSet<Charge>();
            GridAreas = new HashSet<GridArea>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(35)]
        public string MarketParticipantId { get; set; }

        public int BusinessProcessRole { get; set; }

        public bool IsActive { get; set; }

        [InverseProperty(nameof(Charge.Owner))]
        public virtual ICollection<Charge> Charges { get; set; }

        [InverseProperty(nameof(GridArea.GridAccessProvider))]
        public virtual ICollection<GridArea> GridAreas { get; set; }
    }
}
