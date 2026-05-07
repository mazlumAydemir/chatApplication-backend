using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces
{
    public interface IAdminService
    {
        // Sistem istatistiklerini getirir
        Task<AdminStatsDto> GetSystemStatsAsync();

        // Kullanıcıyı banlar veya kaldırır
        Task<bool> ToggleUserBanAsync(Guid userId);

        // Tüm kullanıcıları listeler
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}