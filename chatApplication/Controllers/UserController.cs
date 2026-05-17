using chatApplication.Application.DTOs;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapmış ve JWT Token'ı olan kullanıcılar rehberi görebilir
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUsers()
        {
            // İsteği yapan kullanıcının kendi ID'sini token içinden buluyoruz
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // int yerine Guid olarak parse ediyoruz!
            if (!Guid.TryParse(currentUserIdStr, out Guid currentUserId))
                return Unauthorized();

            // Veritabanından, kendisi hariç diğer tüm aktif kullanıcıları çekiyoruz (Email doğrulama şartı kaldırıldı!)
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && u.IsActive) // IsEmailVerified kaldırıldı, sadece aktiflik kontrolü kaldı
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber, // Email yerine PhoneNumber dönüyoruz
                    Role = u.Role.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}