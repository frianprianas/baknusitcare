using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaknusITCare.Data;
using BaknusITCare.DTOs;
using BaknusITCare.Models;

namespace BaknusITCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TechniciansController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TechniciansController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mengambil daftar seluruh Petugas IT (Teknisi) dan Administrator yang telah ditunjuk oleh Admin.
        /// Mengembalikan informasi nama, email, asal unit/staf (Guru/TU), peran, serta statistik tiket yang sedang/telah ditangani.
        /// </summary>
        /// <param name="includeAdmins">Jika true, menyertakan Administrator IT. Default: true.</param>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetAppointedTechnicians([FromQuery] bool includeAdmins = true)
        {
            var query = _dbContext.Users.AsNoTracking().Where(u => u.IsActive);

            if (includeAdmins)
            {
                query = query.Where(u => u.RoleName == "Teknisi" || u.RoleName == "Admin");
            }
            else
            {
                query = query.Where(u => u.RoleName == "Teknisi");
            }

            var users = await query
                .OrderByDescending(u => u.RoleName == "Teknisi")
                .ThenBy(u => u.FullName)
                .ToListAsync();

            // Hitung beban kerja & rekam jejak penyelesaian tiket masing-masing petugas
            var ticketStats = await _dbContext.Tickets
                .Where(t => !string.IsNullOrEmpty(t.AssignedTechnicianId))
                .GroupBy(t => t.AssignedTechnicianId)
                .Select(g => new
                {
                    TechnicianId = g.Key,
                    ActiveCount = g.Count(t => t.Status == TicketStatus.Diproses || t.Status == TicketStatus.MenungguSparepart),
                    ResolvedCount = g.Count(t => t.Status == TicketStatus.Selesai || t.Status == TicketStatus.Ditutup)
                })
                .ToListAsync();

            var statsMap = ticketStats.ToDictionary(s => s.TechnicianId!, s => s);

            var result = users.Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email,
                phoneNumber = u.PhoneNumber ?? "-",
                role = u.RoleName, // "Teknisi" atau "Admin"
                roleLabel = u.RoleName == "Teknisi" ? "Petugas IT (Teknisi)" : "Administrator IT",
                department = u.DepartmentOrClass ?? "Staf",
                activeTicketsHandled = statsMap.ContainsKey(u.Id) ? statsMap[u.Id].ActiveCount : 0,
                totalResolvedTickets = statsMap.ContainsKey(u.Id) ? statsMap[u.Id].ResolvedCount : 0
            }).ToList<object>();

            return Ok(ApiResponse<List<object>>.Ok(result, $"Ditemukan {result.Count} Petugas IT yang telah ditunjuk."));
        }
    }
}
