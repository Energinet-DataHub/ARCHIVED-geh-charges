using System;
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model.Scaffolded
{
    public partial class MarketParticipant
    {
        public MarketParticipant()
        {
            Charges = new HashSet<Charge>();
        }

        public Guid Id { get; set; }
        public string MarketParticipantId { get; set; }
        public int BusinessProcessRole { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Charge> Charges { get; set; }
    }
}
