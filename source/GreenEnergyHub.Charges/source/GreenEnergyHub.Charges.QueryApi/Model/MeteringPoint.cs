using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("MeteringPoint", Schema = "Charges")]
    [Index("GridAreaLinkId", Name = "IX_GridAreaLinkId")]
    [Index("MeteringPointId", Name = "IX_MeteringPointId")]
    [Index("MeteringPointId", Name = "UC_MeteringPointId", IsUnique = true)]
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

        [ForeignKey("GridAreaLinkId")]
        [InverseProperty("MeteringPoints")]
        public virtual GridAreaLink GridAreaLink { get; set; }
        [InverseProperty("MeteringPoint")]
        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }
    }
}
