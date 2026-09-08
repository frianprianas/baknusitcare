using System.Collections.Generic;
using System.Threading.Tasks;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public class DashboardStats
    {
        public int TotalTickets { get; set; }
        public int NewTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int PendingPartsTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int OverdueSlaTickets { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<string, int> CategoryBreakdown { get; set; } = new();
    }

    public interface ITicketService
    {
        Task<List<Ticket>> GetTicketsAsync(string? status = null, string? category = null, string? search = null, string? userId = null, bool isTechnicianOrAdmin = false);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<Ticket?> GetTicketByCodeAsync(string code);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<bool> UpdateStatusAsync(int ticketId, TicketStatus newStatus, string updatedByUserId, string updatedByName, string userRole, string? comment = null);
        Task<bool> AssignTechnicianAsync(int ticketId, string technicianId, string technicianName, string assignedByUserId);
        Task<bool> AddCommentAsync(int ticketId, string userId, string userName, string userRole, string message, bool isInternal);
        Task<bool> SubmitRatingAsync(int ticketId, int rating, string? feedback);
        Task<DashboardStats> GetDashboardStatsAsync(string? userId = null, bool isTechnicianOrAdmin = false);
        Task<List<TicketCategory>> GetCategoriesAsync();
    }
}
