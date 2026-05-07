using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Infrastructure.Contexts;

namespace chatApplication.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            return new UserProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UserProfileDto profileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.Bio = profileDto.Bio;
            // Fotoğraf URL'sini sadece dışarıdan gelmişse güncelle (eskiyi silmemek için)
            if (!string.IsNullOrEmpty(profileDto.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = profileDto.ProfilePictureUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}