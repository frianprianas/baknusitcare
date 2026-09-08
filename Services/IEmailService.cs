using System.Threading.Tasks;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public interface IEmailService
    {
        Task<bool> SendTicketCreatedConfirmationAsync(Ticket ticket);
        Task<bool> SendNewTicketAlertToTimITAsync(Ticket ticket, System.Collections.Generic.List<string> timItEmails);
        Task<bool> SendTicketStatusUpdatedAsync(Ticket ticket, string oldStatus, string newStatus, string? commentMessage = null);
        Task<bool> SendTicketAssignedAsync(Ticket ticket, string technicianName, string technicianEmail);
    }
}
