using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    [Table("OutboxMessage", Schema = "Charges")]
    public partial class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Type { get; set; }
        [Required]
        public string Data { get; set; }
        [Required]
        [StringLength(255)]
        public string CorrelationId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}
