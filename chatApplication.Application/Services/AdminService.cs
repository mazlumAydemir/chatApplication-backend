using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class AdminService : IAdminService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<SystemLog> _logRepository; // Log repository

    public AdminService(
        IGenericRepository<User> userRepository,
        IGenericRepository<Message> messageRepository,
        IGenericRepository<SystemLog> logRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _logRepository = logRepository;
    }

    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var allUsers = await _userRepository.GetAllAsync();
        var allMessages = await _messageRepository.GetAllAsync();

        // Log ekle
        await _logRepository.AddAsync(new SystemLog { EventType = "Info", Message = "Admin panel istatistikleri görüntülendi.", Timestamp = DateTime.UtcNow });

        return new SystemStatsDto
        {
            ServerStatus = "Online",
            TotalUsers = allUsers.Count(),
            TotalMessagesExchanged = allMessages.Count(),
            ClientUsers = allUsers.Count(u => u.Role == "Client"),
            SysadminUsers = allUsers.Count(u => u.Role == "Sysadmin"),
            LatestActivity = allMessages.OrderByDescending(m => m.SentAt).FirstOrDefault()?.SentAt
        };
    }

    public async Task<List<SystemLogDto>> GetLogsAsync()
    {
        var logs = await _logRepository.GetAllAsync();
        return logs.OrderByDescending(x => x.Timestamp)
                   .Take(50)
                   .Select(x => new SystemLogDto { EventType = x.EventType, Message = x.Message, Timestamp = x.Timestamp })
                   .ToList();
    }

    public async Task TerminateUserSessionAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.IsOnline = false;
            await _userRepository.UpdateAsync(user);
            await _logRepository.AddAsync(new SystemLog { EventType = "Security", Message = $"Admin, {user.PhoneNumber} oturumunu sonlandırdı.", Timestamp = DateTime.UtcNow });
        }
    }
}