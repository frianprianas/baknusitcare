using System;
using BaknusITCare.Models;

namespace BaknusITCare.Services
{
    public class UserSessionService
    {
        public ApplicationUser? CurrentUser { get; private set; }

        public event Action? OnSessionChanged;

        public void SetUser(ApplicationUser user)
        {
            CurrentUser = user;
            NotifySessionChanged();
        }

        public void Logout()
        {
            CurrentUser = null;
            NotifySessionChanged();
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsAdmin => CurrentUser?.RoleName == "Admin";
        public bool IsTechnician => CurrentUser?.RoleName == "Teknisi";
        public bool IsTechnicianOrAdmin => IsAdmin || IsTechnician;

        private void NotifySessionChanged() => OnSessionChanged?.Invoke();
    }
}
