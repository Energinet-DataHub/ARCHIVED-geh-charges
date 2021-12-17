using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class DefaultChargeLink
    {
        public Guid Id { get; set; }
        public int MeteringPointType { get; set; }
        public Guid ChargeId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public virtual Charge Charge { get; set; }
    }
}
