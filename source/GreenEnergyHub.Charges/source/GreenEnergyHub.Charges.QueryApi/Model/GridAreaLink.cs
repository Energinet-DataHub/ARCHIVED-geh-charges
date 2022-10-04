using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("GridAreaLink", Schema = "Charges")]
    [Index("Id", Name = "IX_GridAreaLinkId")]
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

        [ForeignKey("OwnerId")]
        [InverseProperty("GridAreaLinks")]
        public virtual MarketParticipant Owner { get; set; }
        [InverseProperty("GridAreaLink")]
        public virtual ICollection<MeteringPoint> MeteringPoints { get; set; }
    }
}
