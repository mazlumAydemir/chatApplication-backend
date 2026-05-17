using BCrypt.Net;
using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Domain.Entities;
using chatApplication.Domain.Enums;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        // NOT: E-posta doğrulaması kaldırıldığı için IEmailService bağımlılığı temizlendi.
        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Bu telefon numarası ile daha önce kayıt olunmuş mu kontrol et
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                return BadRequest("Bu telefon numarası zaten kullanımda.");

            // 2. Yeni kullanıcı nesnesini oluştur
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                // Şifreyi BCrypt ile hashliyoruz
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = (UserRole)request.RoleId
            };

            // 3. Veritabanına kaydet
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 4. Doğrulama adımı olmadan direkt başarı mesajı dönüyoruz
            return Ok(new { Message = "Kayıt başarıyla tamamlandı. Giriş yapabilirsiniz." });
        }

        // NOT: "verify-email" uç noktası e-posta doğrulaması iptal edildiği için tamamen kaldırılmıştır.

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Kullanıcıyı telefon numarasından bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            // 2. Kullanıcı yoksa veya şifre eşleşmiyorsa (BCrypt ile doğruluyoruz)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest("Hatalı telefon numarası veya şifre.");

            // NOT: E-posta doğrulama kontrolü (IsEmailVerified) tamamen kaldırıldı.

            // 3. Şifre doğruysa JWT Token oluştur
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token içine kullanıcının kimlik bilgilerini (Claims) gömüyoruz
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber), // Email yerine MobilePhone claim'i eklendi
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Token 2 saat geçerli
                signingCredentials: creds
            );

            // Üretilen token'ı geri döndür
            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Message = "Giriş başarılı."
            });
        }
    }
}