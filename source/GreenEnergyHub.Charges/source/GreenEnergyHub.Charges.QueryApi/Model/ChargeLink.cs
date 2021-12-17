using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class ChargeLink
    {
        public Guid Id { get; set; }
        public Guid ChargeId { get; set; }
        public Guid MeteringPointId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Factor { get; set; }

        public virtual Charge Charge { get; set; }
        public virtual MeteringPoint MeteringPoint { get; set; }
    }
}
