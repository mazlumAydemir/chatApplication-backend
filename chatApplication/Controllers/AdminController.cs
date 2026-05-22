using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Sysadmin")] // Sadece Sysadmin erişebilir
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // 1. İstatistikleri Getir (Monitoring Paneli İçin)
    [HttpGet("monitoring")]
    public async Task<IActionResult> GetSystemStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(stats);
    }

    // 2. Sistem Loglarını Getir (Güvenlik Denetimi İçin)
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _adminService.GetLogsAsync();
        return Ok(logs);
    }

    // 3. Kullanıcı Oturumunu Sonlandır (Yönetim Yetkisi)
    [HttpPost("terminate/{userId}")]
    public async Task<IActionResult> TerminateSession(int userId)
    {
        await _adminService.TerminateUserSessionAsync(userId);
        return Ok(new { message = $"User {userId} session terminated successfully." });
    }
}