using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace chatApplication.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminStatsDto> GetSystemStatsAsync()
        {
            return new AdminStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(), // Toplam kullanıcı sayısı[cite: 1]
                TotalMessages = await _context.Messages.CountAsync(), // Toplam mesaj hacmi[cite: 1]
                // Not: OnlineUsers verisi SignalR Hub üzerinden statik olarak çekilecek
            };
        }

        public async Task<bool> ToggleUserBanAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive; // Kullanıcıyı askıya al veya aktif et[cite: 1]
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt // EKSİK OLAN VE EKLENMESİ GEREKEN SATIR
                })
                .ToListAsync();
        }

    }
}