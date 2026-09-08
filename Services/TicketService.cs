using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BaknusITCare.Data;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ApplicationDbContext dbContext,
            IEmailService emailService,
            Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory,
            ILogger<TicketService> logger)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<List<Ticket>> GetTicketsAsync(string? status = null, string? category = null, string? search = null, string? userId = null, bool isTechnicianOrAdmin = false)
        {
            try
            {
                var query = _dbContext.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Comments)
                    .Include(t => t.Attachments)
                    .AsQueryable();

                if (!isTechnicianOrAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.RequesterId == userId);
                }

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(t => t.Status == statusEnum);
                }

                if (!string.IsNullOrEmpty(category) && int.TryParse(category, out var catId))
                {
                    query = query.Where(t => t.CategoryId == catId);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(t => 
                        t.TicketCode.ToLower().Contains(search) ||
                        t.Title.ToLower().Contains(search) ||
                        t.Location.ToLower().Contains(search) ||
                        t.RequesterName.ToLower().Contains(search) ||
                        (t.AssetTag != null && t.AssetTag.ToLower().Contains(search))
                    );
                }

                return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil daftar tiket dari database.");
                return new List<Ticket>();
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Comments)
                    .Include(t => t.Attachments)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil detail tiket #{Id}", id);
                return null;
            }
        }

        public async Task<Ticket?> GetTicketByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            string cleanCode = code.Trim().TrimStart('#').ToUpper();

            try
            {
                return await _dbContext.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Comments)
                    .Include(t => t.Attachments)
                    .FirstOrDefaultAsync(t => t.TicketCode.ToUpper() == cleanCode 
                                           || t.TicketCode.ToUpper() == $"NET-{cleanCode}"
                                           || t.TicketCode.ToUpper() == $"BID-{cleanCode}"
                                           || t.TicketCode.EndsWith($"-{cleanCode}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil tiket kode {Code}", code);
                return null;
            }
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            TicketCategory? category = null;
            try
            {
                category = await _dbContext.TicketCategories.FindAsync(ticket.CategoryId);
            }
            catch { }

            // Tentukan prefix nomor tiket yang mudah dibaca & dilacak:
            // NET-101 (Layanan Internet), BID-101 (Layanan BaknusID)
            string prefix = "NET";
            if (category != null && (
                category.Name.Contains("Baknus", StringComparison.OrdinalIgnoreCase) || 
                category.Name.Contains("ID", StringComparison.OrdinalIgnoreCase) ||
                category.Name.Contains("Software", StringComparison.OrdinalIgnoreCase)))
            {
                prefix = "BID";
            }
            else
            {
                prefix = "NET";
            }

            int countForPrefix = 0;
            try
            {
                countForPrefix = await _dbContext.Tickets.CountAsync(t => t.TicketCode.StartsWith(prefix));
            }
            catch { }

            int nextNum = 101 + countForPrefix;
            while (await _dbContext.Tickets.AnyAsync(t => t.TicketCode == $"{prefix}-{nextNum}"))
            {
                nextNum++;
            }
            ticket.TicketCode = $"{prefix}-{nextNum}";

            int slaHours = category?.DefaultSlaHours ?? (prefix == "NET" ? 2 : 4);
            ticket.DueDate = DateTime.UtcNow.AddHours(slaHours);
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.Baru;

            await _dbContext.Tickets.AddAsync(ticket);
            await _dbContext.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    ticket.Category = category;
                    // 1. Kirim email konfirmasi tanda terima ke Pembuat Tiket (Pelapor)
                    await _emailService.SendTicketCreatedConfirmationAsync(ticket);

                    // 2. Ambil seluruh anggota Tim IT (Teknisi) dan Admin untuk dikirimi alert email
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var timItEmails = await db.Users
                        .Where(u => u.RoleName == "Teknisi" || u.RoleName == "Admin")
                        .Where(u => !string.IsNullOrEmpty(u.Email))
                        .Select(u => u.Email)
                        .Distinct()
                        .ToListAsync();

                    if (timItEmails.Any())
                    {
                        await _emailService.SendNewTicketAlertToTimITAsync(ticket, timItEmails);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gagal mengirim email notifikasi tiket #{TicketCode}", ticket.TicketCode);
                }
            });

            return ticket;
        }

        public async Task<bool> UpdateStatusAsync(int ticketId, TicketStatus newStatus, string updatedByUserId, string updatedByName, string userRole, string? comment = null)
        {
            var ticket = await GetTicketByIdAsync(ticketId);
            if (ticket == null) return false;

            string oldStatusStr = ticket.Status.ToString();
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (newStatus == TicketStatus.Selesai || newStatus == TicketStatus.Ditutup)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }

            var statusComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = updatedByUserId,
                UserName = updatedByName,
                UserRole = userRole,
                Message = !string.IsNullOrWhiteSpace(comment) 
                    ? $"[Status diubah ke {newStatus}] {comment}"
                    : $"Status tiket diperbarui dari {oldStatusStr} menjadi {newStatus}.",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.TicketComments.AddAsync(statusComment);
            await _dbContext.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendTicketStatusUpdatedAsync(ticket, oldStatusStr, newStatus.ToString(), comment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gagal mengirim notifikasi status email untuk tiket #{TicketCode}", ticket.TicketCode);
                }
            });

            return true;
        }

        public async Task<bool> AssignTechnicianAsync(int ticketId, string technicianId, string technicianName, string assignedByUserId)
        {
            var ticket = await GetTicketByIdAsync(ticketId);
            if (ticket == null) return false;

            ticket.AssignedTechnicianId = technicianId;
            ticket.AssignedTechnicianName = technicianName;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (ticket.Status == TicketStatus.Baru)
            {
                ticket.Status = TicketStatus.Diproses;
            }

            var techUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == technicianId);
            string techEmail = techUser?.Email ?? "";

            var assignComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = assignedByUserId,
                UserName = "Sistem BaknusITCare",
                UserRole = "System",
                Message = $"Tiket telah ditugaskan kepada: {technicianName}.",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.TicketComments.AddAsync(assignComment);
            await _dbContext.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(techEmail))
                    {
                        await _emailService.SendTicketAssignedAsync(ticket, technicianName, techEmail);
                    }
                    // Juga kirimkan notifikasi ke Pembuat Tiket (Pelapor) bahwa tiket mulai ditangani
                    await _emailService.SendTicketStatusUpdatedAsync(ticket, "Baru", "Diproses", $"Tiket telah ditugaskan kepada {technicianName} dan mulai dalam proses penanganan.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gagal mengirim email penugasan / status untuk tiket #{TicketCode}", ticket.TicketCode);
                }
            });

            return true;
        }

        public async Task<bool> AddCommentAsync(int ticketId, string userId, string userName, string userRole, string message, bool isInternal)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                Message = message.Trim(),
                IsInternal = isInternal,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.TicketComments.AddAsync(comment);
            
            var ticket = await _dbContext.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                ticket.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitRatingAsync(int ticketId, int rating, string? feedback)
        {
            var ticket = await _dbContext.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.Rating = Math.Clamp(rating, 1, 5);
            ticket.FeedbackComments = feedback;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(string? userId = null, bool isTechnicianOrAdmin = false)
        {
            try
            {
                var query = _dbContext.Tickets.AsQueryable();
                if (!isTechnicianOrAdmin && !string.IsNullOrEmpty(userId))
                {
                    query = query.Where(t => t.RequesterId == userId);
                }

                var allTickets = await query.Include(t => t.Category).ToListAsync();

                var stats = new DashboardStats
                {
                    TotalTickets = allTickets.Count,
                    NewTickets = allTickets.Count(t => t.Status == TicketStatus.Baru),
                    InProgressTickets = allTickets.Count(t => t.Status == TicketStatus.Diproses),
                    PendingPartsTickets = allTickets.Count(t => t.Status == TicketStatus.MenungguSparepart),
                    ResolvedTickets = allTickets.Count(t => t.Status == TicketStatus.Selesai || t.Status == TicketStatus.Ditutup),
                    OverdueSlaTickets = allTickets.Count(t => t.IsOverdue),
                    AverageRating = allTickets.Where(t => t.Rating.HasValue).Select(t => t.Rating!.Value).DefaultIfEmpty(0).Average()
                };

                var catGroups = allTickets
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category!.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.CategoryBreakdown = catGroups;
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil statistik dashboard.");
                return new DashboardStats();
            }
        }

        public async Task<List<TicketCategory>> GetCategoriesAsync()
        {
            try
            {
                return await _dbContext.TicketCategories.Where(c => c.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mengambil daftar kategori.");
                return new List<TicketCategory>();
            }
        }
    }
}
