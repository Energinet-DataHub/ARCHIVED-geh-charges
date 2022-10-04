using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("Charge", Schema = "Charges")]
    [Index("SenderProvidedChargeId", "Type", "OwnerId", Name = "IX_SenderProvidedChargeId_ChargeType_MarketParticipantId")]
    [Index("SenderProvidedChargeId", "Type", "OwnerId", Name = "UC_SenderProvidedChargeId_Type_OwnerId", IsUnique = true)]
    public partial class Charge
    {
        public Charge()
        {
            ChargeLinks = new HashSet<ChargeLink>();
            ChargePeriods = new HashSet<ChargePeriod>();
            ChargePoints = new HashSet<ChargePoint>();
            DefaultChargeLinks = new HashSet<DefaultChargeLink>();
        }

        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; set; }
        public int Type { get; set; }
        public Guid OwnerId { get; set; }
        public bool TaxIndicator { get; set; }
        public int Resolution { get; set; }

        [ForeignKey("OwnerId")]
        [InverseProperty("Charges")]
        public virtual MarketParticipant Owner { get; set; }
        [InverseProperty("Charge")]
        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }
        [InverseProperty("Charge")]
        public virtual ICollection<ChargePeriod> ChargePeriods { get; set; }
        [InverseProperty("Charge")]
        public virtual ICollection<ChargePoint> ChargePoints { get; set; }
        [InverseProperty("Charge")]
        public virtual ICollection<DefaultChargeLink> DefaultChargeLinks { get; set; }
    }
}
