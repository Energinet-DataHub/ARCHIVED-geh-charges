using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("ChargeLink", Schema = "Charges")]
    [Index("MeteringPointId", "ChargeId", Name = "IX_MeteringPointId_ChargeId")]
    [Index("ChargeId", "MeteringPointId", "EndDateTime", Name = "UQ_DefaultOverlap_EndDateTime", IsUnique = true)]
    [Index("ChargeId", "MeteringPointId", "StartDateTime", Name = "UQ_DefaultOverlap_StartDateTime", IsUnique = true)]
    public partial class ChargeLink
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ChargeId { get; set; }
        public Guid MeteringPointId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Factor { get; set; }

        [ForeignKey("ChargeId")]
        [InverseProperty("ChargeLinks")]
        public virtual Charge Charge { get; set; }
        [ForeignKey("MeteringPointId")]
        [InverseProperty("ChargeLinks")]
        public virtual MeteringPoint MeteringPoint { get; set; }
    }
}
