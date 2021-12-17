using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class MeteringPoint
    {
        public MeteringPoint()
        {
            ChargeLinks = new HashSet<ChargeLink>();
        }

        public Guid Id { get; set; }
        public string MeteringPointId { get; set; }
        public int MeteringPointType { get; set; }
        public string GridAreaId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int ConnectionState { get; set; }
        public int? SettlementMethod { get; set; }

        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }
    }
}
