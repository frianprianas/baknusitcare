using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaknusITCare.Models
{
    public enum TicketStatus
    {
        Baru = 0,
        Diproses = 1,
        MenungguSparepart = 2,
        Selesai = 3,
        Ditutup = 4
    }

    public enum TicketPriority
    {
        Rendah = 0,
        Sedang = 1,
        Tinggi = 2,
        Darurat = 3
    }

    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string TicketCode { get; set; } = string.Empty; // e.g. IT-202609-001

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public TicketCategory? Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty; // e.g. Lab Komputer 1, Ruang Guru

        [StringLength(100)]
        public string? AssetTag { get; set; } // e.g. PC-LAB1-05, PRN-TU-01

        public TicketPriority Priority { get; set; } = TicketPriority.Sedang;

        public TicketStatus Status { get; set; } = TicketStatus.Baru;

        [Required]
        [StringLength(100)]
        public string RequesterId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string RequesterName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string RequesterEmail { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssignedTechnicianId { get; set; }

        [StringLength(150)]
        public string? AssignedTechnicianName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime DueDate { get; set; }

        // Customer Rating & Feedback
        public int? Rating { get; set; } // 1 - 5 stars

        [StringLength(500)]
        public string? FeedbackComments { get; set; }

        // Navigation Collections
        public List<TicketComment> Comments { get; set; } = new();
        public List<TicketAttachment> Attachments { get; set; } = new();

        [NotMapped]
        public bool IsOverdue => Status != TicketStatus.Selesai && Status != TicketStatus.Ditutup && DateTime.UtcNow > DueDate;

        [NotMapped]
        public TimeSpan TimeRemaining => Status != TicketStatus.Selesai && Status != TicketStatus.Ditutup 
            ? (DueDate > DateTime.UtcNow ? DueDate - DateTime.UtcNow : TimeSpan.Zero)
            : TimeSpan.Zero;
    }
}
