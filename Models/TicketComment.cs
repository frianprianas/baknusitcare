using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaknusITCare.Models
{
    public class TicketComment
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public Ticket? Ticket { get; set; }

        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserRole { get; set; } = "Pelapor";

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false; // Internal notes (Technician only) vs Public reply

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
