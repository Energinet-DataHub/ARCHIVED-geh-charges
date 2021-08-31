using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    public class DefaultChargeLinkSetting
    {
        [Key]
        public int RowId { get; set; }

        [ForeignKey("Charge")]
        public int ChargeRowId { get; set; }

        public int MeteringPointType { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public virtual Charge Charge { get; set; }
    }
}
