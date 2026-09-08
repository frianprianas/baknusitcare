using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BaknusITCare.DTOs;
using BaknusITCare.Models;
using BaknusITCare.Services;

namespace BaknusITCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Mengambil daftar tiket.
        /// - Jika pelapor: kirimkan `userId` untuk hanya mengambil tiket miliknya.
        /// - Jika petugas IT/Admin: kirimkan `role=Teknisi` atau `role=Admin` untuk melihat semua tiket.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetTickets(
            [FromQuery] string? userId = null,
            [FromQuery] string? role = "Pelapor",
            [FromQuery] string? status = null,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            bool isTechOrAdmin = role?.Equals("Teknisi", StringComparison.OrdinalIgnoreCase) == true ||
                                 role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            var tickets = await _ticketService.GetTicketsAsync(status, category, search, userId, isTechOrAdmin);

            var result = tickets.Select(t => new
            {
                id = t.Id,
                ticketCode = t.TicketCode,
                title = t.Title,
                category = t.Category?.Name ?? "Umum",
                location = t.Location,
                priority = t.Priority.ToString(),
                status = t.Status.ToString(),
                requesterName = t.RequesterName,
                requesterEmail = t.RequesterEmail,
                assignedTechnicianName = t.AssignedTechnicianName ?? "Belum Ditugaskan",
                createdAt = t.CreatedAt,
                dueDate = t.DueDate,
                isOverdue = t.IsOverdue,
                commentsCount = t.Comments?.Count ?? 0,
                rating = t.Rating
            }).ToList<object>();

            return Ok(ApiResponse<List<object>>.Ok(result, $"Ditemukan {result.Count} tiket."));
        }

        /// <summary>
        /// Mengambil ringkasan statistik dashboard untuk aplikasi mobile
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStats>>> GetDashboardStats(
            [FromQuery] string? userId = null,
            [FromQuery] string? role = "Pelapor")
        {
            bool isTechOrAdmin = role?.Equals("Teknisi", StringComparison.OrdinalIgnoreCase) == true ||
                                 role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            var stats = await _ticketService.GetDashboardStatsAsync(userId, isTechOrAdmin);
            return Ok(ApiResponse<DashboardStats>.Ok(stats, "Statistik dashboard berhasil dimuat."));
        }

        /// <summary>
        /// Mengambil detail lengkap 1 tiket berdasarkan ID database
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Tiket ID {id} tidak ditemukan."));
            }

            var detail = new
            {
                id = ticket.Id,
                ticketCode = ticket.TicketCode,
                title = ticket.Title,
                description = ticket.Description,
                category = ticket.Category?.Name ?? "Umum",
                location = ticket.Location,
                assetTag = ticket.AssetTag,
                priority = ticket.Priority.ToString(),
                status = ticket.Status.ToString(),
                requesterId = ticket.RequesterId,
                requesterName = ticket.RequesterName,
                requesterEmail = ticket.RequesterEmail,
                assignedTechnicianId = ticket.AssignedTechnicianId,
                assignedTechnicianName = ticket.AssignedTechnicianName ?? "Belum Ditugaskan",
                createdAt = ticket.CreatedAt,
                updatedAt = ticket.UpdatedAt,
                resolvedAt = ticket.ResolvedAt,
                dueDate = ticket.DueDate,
                isOverdue = ticket.IsOverdue,
                rating = ticket.Rating,
                feedbackComments = ticket.FeedbackComments,
                comments = ticket.Comments.OrderBy(c => c.CreatedAt).Select(c => new
                {
                    id = c.Id,
                    userId = c.UserId,
                    userName = c.UserName,
                    userRole = c.UserRole,
                    message = c.Message,
                    isInternal = c.IsInternal,
                    createdAt = c.CreatedAt
                }).ToList()
            };

            return Ok(ApiResponse<object>.Ok(detail, "Detail tiket berhasil diambil."));
        }

        /// <summary>
        /// Melacak status tiket secara publik menggunakan kode tiket (misal: NET-101, BID-102)
        /// </summary>
        [HttpGet("track/{code}")]
        public async Task<ActionResult<ApiResponse<object>>> TrackTicketByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ApiResponse<object>.Fail("Nomor tiket tidak boleh kosong."));
            }

            var ticket = await _ticketService.GetTicketByCodeAsync(code.Trim().ToUpper());
            if (ticket == null)
            {
                return NotFound(ApiResponse<object>.Fail($"Tiket #{code} tidak ditemukan di sistem."));
            }

            var trackData = new
            {
                id = ticket.Id,
                ticketCode = ticket.TicketCode,
                title = ticket.Title,
                description = ticket.Description,
                category = ticket.Category?.Name ?? "Umum",
                location = ticket.Location,
                priority = ticket.Priority.ToString(),
                status = ticket.Status.ToString(),
                requesterName = ticket.RequesterName,
                assignedTechnicianName = ticket.AssignedTechnicianName ?? "Belum Ditugaskan",
                createdAt = ticket.CreatedAt,
                dueDate = ticket.DueDate,
                resolvedAt = ticket.ResolvedAt,
                isOverdue = ticket.IsOverdue,
                comments = ticket.Comments.Where(c => !c.IsInternal).OrderBy(c => c.CreatedAt).Select(c => new
                {
                    userName = c.UserName,
                    userRole = c.UserRole,
                    message = c.Message,
                    createdAt = c.CreatedAt
                }).ToList()
            };

            return Ok(ApiResponse<object>.Ok(trackData, "Data pelacakan tiket berhasil ditemukan."));
        }

        /// <summary>
        /// Membuat tiket laporan kendala baru (Internet atau BaknusID).
        /// Otomatis mengirimkan email alert ke Tim IT dan konfirmasi ke pelapor.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateTicket([FromBody] CreateTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RequesterName) || string.IsNullOrWhiteSpace(request.RequesterEmail))
            {
                return BadRequest(ApiResponse<object>.Fail("Informasi pelapor (nama dan email) wajib diisi."));
            }

            var categories = await _ticketService.GetCategoriesAsync();
            int categoryId = 0;

            if (request.ServiceType.Equals("Internet", StringComparison.OrdinalIgnoreCase))
            {
                var cat = categories.FirstOrDefault(c => c.Name.Contains("Internet", StringComparison.OrdinalIgnoreCase))
                          ?? categories.FirstOrDefault(c => c.Name.Contains("Jaringan", StringComparison.OrdinalIgnoreCase))
                          ?? categories.FirstOrDefault();
                if (cat != null) categoryId = cat.Id;
            }
            else
            {
                var cat = categories.FirstOrDefault(c => c.Name.Contains("Baknus", StringComparison.OrdinalIgnoreCase) || c.Name.Contains("ID", StringComparison.OrdinalIgnoreCase))
                          ?? categories.FirstOrDefault(c => c.Name.Contains("Software", StringComparison.OrdinalIgnoreCase))
                          ?? categories.LastOrDefault();
                if (cat != null) categoryId = cat.Id;
            }

            // Parsing Priority
            var priority = TicketPriority.Sedang;
            if (Enum.TryParse<TicketPriority>(request.Priority, true, out var p))
            {
                priority = p;
            }

            var ticket = new Ticket
            {
                CategoryId = categoryId,
                Priority = priority,
                RequesterId = request.RequesterId,
                RequesterName = request.RequesterName,
                RequesterEmail = request.RequesterEmail,
                Status = TicketStatus.Baru
            };

            if (request.ServiceType.Equals("Internet", StringComparison.OrdinalIgnoreCase))
            {
                string subIssue = !string.IsNullOrWhiteSpace(request.SubIssue) ? request.SubIssue : "Kendala Koneksi Internet";
                string loc = !string.IsNullOrWhiteSpace(request.Location) ? request.Location : "Area Sekolah";
                ticket.Title = $"[Internet] {subIssue} di {loc}";
                ticket.Location = loc;
                ticket.Description = request.Description ?? subIssue;
            }
            else
            {
                string appName = !string.IsNullOrWhiteSpace(request.BaknusIdApp) ? request.BaknusIdApp : "Layanan BaknusID";
                ticket.Title = $"[BaknusID - {appName}] Kendala Akses Layanan Digital";
                ticket.Location = "Online / Layanan Cloud BaknusID";
                ticket.Description = !string.IsNullOrWhiteSpace(request.BaknusIdNote)
                    ? $"Kendala pada layanan {appName}. Catatan: {request.BaknusIdNote.Trim()}"
                    : $"Laporan kendala pada layanan {appName} atas nama {request.RequesterName}.";
            }

            var created = await _ticketService.CreateTicketAsync(ticket);

            var responseData = new
            {
                id = created.Id,
                ticketCode = created.TicketCode,
                title = created.Title,
                status = created.Status.ToString(),
                dueDate = created.DueDate,
                createdAt = created.CreatedAt
            };

            return CreatedAtAction(nameof(GetTicketById), new { id = created.Id }, 
                ApiResponse<object>.Ok(responseData, $"Tiket #{created.TicketCode} berhasil dibuat dan disiarkan ke Petugas IT."));
        }

        /// <summary>
        /// Mengambil riwayat komentar / tanya jawab pada suatu tiket
        /// </summary>
        [HttpGet("{id:int}/comments")]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetComments(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound(ApiResponse<List<object>>.Fail($"Tiket ID {id} tidak ditemukan."));
            }

            var comments = ticket.Comments.OrderBy(c => c.CreatedAt).Select(c => new
            {
                id = c.Id,
                userId = c.UserId,
                userName = c.UserName,
                userRole = c.UserRole,
                message = c.Message,
                isInternal = c.IsInternal,
                createdAt = c.CreatedAt
            }).ToList<object>();

            return Ok(ApiResponse<List<object>>.Ok(comments, "Riwayat komentar berhasil dimuat."));
        }

        /// <summary>
        /// Menambahkan komentar / pesan tanya jawab pada tiket
        /// </summary>
        [HttpPost("{id:int}/comments")]
        public async Task<ActionResult<ApiResponse<bool>>> AddComment(int id, [FromBody] AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<bool>.Fail("Pesan komentar tidak boleh kosong."));
            }

            bool success = await _ticketService.AddCommentAsync(
                id,
                request.UserId,
                request.UserName,
                request.UserRole,
                request.Message.Trim(),
                request.IsInternal
            );

            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail("Gagal mengirimkan komentar. Tiket tidak ditemukan."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Komentar berhasil dikirimkan."));
        }

        /// <summary>
        /// Memperbarui status penanganan tiket oleh Petugas IT (Diproses, Selesai, Ditutup).
        /// Otomatis mengirimkan email update status ke pelapor dan tim IT.
        /// </summary>
        [HttpPost("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!Enum.TryParse<TicketStatus>(request.NewStatus, true, out var newStatus))
            {
                return BadRequest(ApiResponse<bool>.Fail($"Status '{request.NewStatus}' tidak valid. Pilihan: Baru, Diproses, MenungguSparepart, Selesai, Ditutup."));
            }

            bool success = await _ticketService.UpdateStatusAsync(
                id,
                newStatus,
                request.UpdatedByUserId,
                request.UpdatedByName,
                request.UserRole,
                request.Comment
            );

            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail("Gagal memperbarui status. Tiket tidak ditemukan."));
            }

            return Ok(ApiResponse<bool>.Ok(true, $"Status tiket berhasil diperbarui menjadi '{newStatus}'. Email notifikasi telah dikirimkan."));
        }

        /// <summary>
        /// Menugaskan teknisi untuk menangani tiket
        /// </summary>
        [HttpPost("{id:int}/assign")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignTechnician(int id, [FromBody] AssignTechnicianRequest request)
        {
            bool success = await _ticketService.AssignTechnicianAsync(
                id,
                request.TechnicianId,
                request.TechnicianName,
                request.AssignedByUserId
            );

            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail("Gagal menugaskan teknisi. Tiket tidak ditemukan."));
            }

            return Ok(ApiResponse<bool>.Ok(true, $"Teknisi {request.TechnicianName} berhasil ditugaskan pada tiket ini."));
        }

        /// <summary>
        /// Memberikan penilaian kepuasan (Rating bintang 1-5 dan ulasan) setelah tiket selesai
        /// </summary>
        [HttpPost("{id:int}/rating")]
        public async Task<ActionResult<ApiResponse<bool>>> SubmitRating(int id, [FromBody] SubmitRatingRequest request)
        {
            bool success = await _ticketService.SubmitRatingAsync(id, request.Rating, request.Feedback);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Fail("Gagal mengirimkan ulasan. Tiket tidak ditemukan."));
            }

            return Ok(ApiResponse<bool>.Ok(true, "Terima kasih atas ulasan dan penilaian yang Anda berikan!"));
        }
    }
}
