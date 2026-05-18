using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class AdminService : IAdminService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Message> _messageRepository;

    public AdminService(IGenericRepository<User> userRepository, IGenericRepository<Message> messageRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }

    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var allUsers = await _userRepository.GetAllAsync();
        var allMessages = await _messageRepository.GetAllAsync();

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
}