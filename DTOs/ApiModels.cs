using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BaknusITCare.Models;

namespace BaknusITCare.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Pelapor"; // Pelapor, Teknisi, Admin
        public string Department { get; set; } = string.Empty; // Guru, TU, Siswa
        public bool IsAdmin => Role == "Admin";
        public bool IsTechnician => Role == "Teknisi" || Role == "Admin";
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }

    public class CreateTicketRequest
    {
        [Required]
        public string ServiceType { get; set; } = "Internet"; // "Internet" or "BaknusID"

        public string? SubIssue { get; set; } // "Internet tidak terkoneksi", "Wi-Fi / LAN tidak nyala", etc.

        public string? Location { get; set; } // "1. Ruang Workshop", etc.

        public string? Description { get; set; }

        public string? Priority { get; set; } = "Sedang"; // Rendah, Sedang, Tinggi, Darurat

        public string? BaknusIdApp { get; set; } // "WebsiteBaknus", "Baknusmail", "BaknusAttend", "BaknusDrive", "BaknusClass"

        public string? BaknusIdNote { get; set; }

        [Required]
        public string RequesterId { get; set; } = string.Empty;

        [Required]
        public string RequesterName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string RequesterEmail { get; set; } = string.Empty;
    }

    public class UpdateStatusRequest
    {
        [Required]
        public string NewStatus { get; set; } = "Diproses"; // Baru, Diproses, MenungguSparepart, Selesai, Ditutup

        [Required]
        public string UpdatedByUserId { get; set; } = string.Empty;

        [Required]
        public string UpdatedByName { get; set; } = string.Empty;

        public string UserRole { get; set; } = "Teknisi";

        public string? Comment { get; set; }
    }

    public class AddCommentRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string UserRole { get; set; } = "Pelapor";

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;
    }

    public class AssignTechnicianRequest
    {
        [Required]
        public string TechnicianId { get; set; } = string.Empty;

        [Required]
        public string TechnicianName { get; set; } = string.Empty;

        [Required]
        public string AssignedByUserId { get; set; } = string.Empty;
    }

    public class SubmitRatingRequest
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Feedback { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Sukses") => new() { Success = true, Message = message, Data = data };
        public static ApiResponse<T> Fail(string message) => new() { Success = false, Message = message, Data = default };
    }
}
