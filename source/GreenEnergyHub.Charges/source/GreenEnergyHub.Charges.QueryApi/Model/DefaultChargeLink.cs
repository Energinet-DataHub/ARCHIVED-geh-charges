using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("DefaultChargeLink", Schema = "Charges")]
    [Index("MeteringPointType", "StartDateTime", "EndDateTime", Name = "IX_MeteringPointType_StartDateTime_EndDateTime")]
    public partial class DefaultChargeLink
    {
        [Key]
        public Guid Id { get; set; }
        public int MeteringPointType { get; set; }
        public Guid ChargeId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        [ForeignKey("ChargeId")]
        [InverseProperty("DefaultChargeLinks")]
        public virtual Charge Charge { get; set; }
    }
}
