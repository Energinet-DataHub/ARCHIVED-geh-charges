using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model.Scaffolded
{
    public partial class ChargePoint
    {
        public Guid Id { get; set; }
        public Guid ChargeId { get; set; }
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public int Position { get; set; }
    }
}
