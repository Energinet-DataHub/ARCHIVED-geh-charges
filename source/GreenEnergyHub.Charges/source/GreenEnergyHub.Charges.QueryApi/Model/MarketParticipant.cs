using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("MarketParticipant", Schema = "Charges")]
    [Index("ActorId", Name = "UC_ActorId", IsUnique = true)]
    [Index("MarketParticipantId", "BusinessProcessRole", Name = "UC_MarketParticipantId_BusinessProcessRole", IsUnique = true)]
    public partial class MarketParticipant
    {
        public MarketParticipant()
        {
            Charges = new HashSet<Charge>();
            GridAreaLinks = new HashSet<GridAreaLink>();
        }

        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(35)]
        public string MarketParticipantId { get; set; }
        public int BusinessProcessRole { get; set; }
        public bool IsActive { get; set; }
        public Guid ActorId { get; set; }
        [Column("B2CActorId")]
        public Guid? B2cactorId { get; set; }

        [InverseProperty("Owner")]
        public virtual ICollection<Charge> Charges { get; set; }
        [InverseProperty("Owner")]
        public virtual ICollection<GridAreaLink> GridAreaLinks { get; set; }
    }
}
