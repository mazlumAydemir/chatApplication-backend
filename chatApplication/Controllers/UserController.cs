using chatApplication.Application.DTOs;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            // Veritabanından, kendisi hariç ve e-postasını doğrulamış diğer tüm kullanıcıları çekiyoruz
            var users = await _context.Users
                .Where(u => u.IsEmailVerified && u.Id != currentUserId) // Artık ikisi de Guid, hata yok!
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}