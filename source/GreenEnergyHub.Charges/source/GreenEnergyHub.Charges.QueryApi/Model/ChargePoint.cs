using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

        [ForeignKey("ChargeId")]
        [InverseProperty("ChargePoints")]
        public virtual Charge Charge { get; set; }
    }
}
