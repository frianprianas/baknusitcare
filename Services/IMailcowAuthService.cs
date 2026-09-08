using System.Threading.Tasks;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public interface IMailcowAuthService
    {
        Task<(bool Success, ApplicationUser? User, string Message)> AuthenticateUserAsync(string email, string password);
        Task<(int TotalSynced, int AdminCount, int TechnicianCount, int RequesterCount, string Message)> SyncMailcowUsersAsync();
    }
}
