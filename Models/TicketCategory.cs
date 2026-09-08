using System.ComponentModel.DataAnnotations;

namespace BaknusITCare.Models
{
    public class TicketCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = "Desktop";

        public int DefaultSlaHours { get; set; } = 4;

        public bool IsActive { get; set; } = true;
    }
}
