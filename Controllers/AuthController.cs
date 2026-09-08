using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaknusITCare.Data;
using BaknusITCare.DTOs;
using BaknusITCare.Services;

namespace BaknusITCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMailcowAuthService _authService;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(IMailcowAuthService authService, ApplicationDbContext dbContext)
        {
            _authService = authService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Login pengguna (Siswa, Guru, TU, Teknisi, Admin) menggunakan kredensial email Mailcow sekolah
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse { Success = false, Message = "Email dan password wajib diisi." });
            }

            var result = await _authService.AuthenticateUserAsync(request.Email.Trim(), request.Password);

            if (!result.Success || result.User == null)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = result.Message });
            }

            var userDto = new UserDto
            {
                Id = result.User.Id,
                Email = result.User.Email,
                FullName = result.User.FullName,
                Role = result.User.RoleName,
                Department = result.User.DepartmentOrClass ?? "Civitas Sekolah"
            };

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login berhasil.",
                User = userDto
            });
        }

        /// <summary>
        /// Ambil data profil pengguna berdasarkan User ID
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(ApiResponse<UserDto>.Fail("UserId diperlukan."));
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.Fail("Pengguna tidak ditemukan."));
            }

            var dto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.RoleName,
                Department = user.DepartmentOrClass ?? "Civitas Sekolah"
            };

            return Ok(ApiResponse<UserDto>.Ok(dto));
        }
    }
}
