using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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
        public DateTime EndDateTime { get; set; }

        [ForeignKey("ChargeId")]
        [InverseProperty("ChargePeriods")]
        public virtual Charge Charge { get; set; }
    }
}
