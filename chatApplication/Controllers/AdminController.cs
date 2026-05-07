using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatApplication.Controllers
{
    // Veritabanındaki "Sysadmin" rolüne izin veriyoruz
    [Authorize(Roles = "Sysadmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        // Bütün iş mantığını daha önce yazdığımız IAdminService'e devrediyoruz
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetSystemStatsAsync();
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("toggle-ban/{userId}")]
        public async Task<IActionResult> ToggleBan(Guid userId)
        {
            var result = await _adminService.ToggleUserBanAsync(userId);
            return result ? Ok() : NotFound();
        }
    }
}