using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model.Scaffolded
{
    public partial class ChargePrice
    {
        public Guid Id { get; set; }
        public Guid ChargeId { get; set; }
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public bool Retired { get; set; }
        public DateTime? RetiredDateTime { get; set; }
        public Guid ChargeOperationId { get; set; }

        public virtual Charge Charge { get; set; }
    }
}
