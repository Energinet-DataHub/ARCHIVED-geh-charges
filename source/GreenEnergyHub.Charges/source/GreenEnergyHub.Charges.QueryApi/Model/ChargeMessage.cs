using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("ChargeMessage", Schema = "Charges")]
    public class ChargeMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; set; }

        public int Type { get; set; }

        [Required]
        [StringLength(35)]
        public string MarketParticipantId { get; set; }

        [Required]
        [StringLength(255)]
        public string MessageId { get; set; }

        public int MessageType { get; set; }

        public DateTime MessageDateTime { get; set; }
    }
}
