using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class Charge
    {
        public Charge()
        {
            ChargeLinks = new HashSet<ChargeLink>();
            DefaultChargeLinks = new HashSet<DefaultChargeLink>();
        }

        public Guid Id { get; set; }
        public string SenderProvidedChargeId { get; set; }
        public int Type { get; set; }
        public Guid OwnerId { get; set; }
        public bool TaxIndicator { get; set; }
        public int Resolution { get; set; }
        public bool TransparentInvoicing { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int VatClassification { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }

        public virtual MarketParticipant Owner { get; set; }
        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }
        public virtual ICollection<DefaultChargeLink> DefaultChargeLinks { get; set; }
    }
}
