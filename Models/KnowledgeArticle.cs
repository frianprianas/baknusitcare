using System;
using System.ComponentModel.DataAnnotations;

namespace BaknusITCare.Models
{
    public class KnowledgeArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Umum";

        [Required]
        [StringLength(300)]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = "Help";

        public int Views { get; set; } = 0;

        public int HelpfulVotes { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
