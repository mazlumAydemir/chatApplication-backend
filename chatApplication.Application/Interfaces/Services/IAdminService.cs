using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces.Services;

public interface IAdminService
{
    // Sistem istatistiklerini getirir
    Task<SystemStatsDto> GetSystemStatsAsync();

    // Logları getirir
    Task<List<SystemLogDto>> GetLogsAsync();

    // Kullanıcı oturumunu sonlandırır
    Task TerminateUserSessionAsync(int userId);
}