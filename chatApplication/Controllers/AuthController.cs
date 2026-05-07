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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Claims;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;
namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        public AuthController(ApplicationDbContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. Bu e-posta ile daha önce kayıt olunmuş mu kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Bu e-posta adresi zaten kullanımda.");

            // 2. 6 haneli rastgele bir doğrulama kodu üret
            var verificationCode = new Random().Next(100000, 999999).ToString();

            // 3. Yeni kullanıcı nesnesini oluştur (Sysadmin veya Client)
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                // Şifreyi BCrypt ile hashliyoruz!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = (UserRole)request.RoleId,
                EmailVerificationCode = verificationCode,
                VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(15) // Kodun ömrü 15 dakika
            };

            // 4. Veritabanına kaydet
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. E-posta servisini kullanarak kodu gönder
            var emailBody = $@"
                <h3>IEA (Image Encryption Application) Projesine Hoş Geldiniz!</h3>
                <p>Sayın {user.FirstName} {user.LastName},</p>
                <p>Doğrulama kodunuz: <b style='font-size:20px; color:blue;'>{verificationCode}</b></p>
                <p>Bu kod 15 dakika boyunca geçerlidir.</p>";

            await _emailService.SendEmailAsync(user.Email, "E-posta Doğrulama Kodu", emailBody);

            return Ok("Kayıt başarılı. Lütfen e-postanıza gönderilen kodu doğrulayın.");
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            // 1. Kullanıcıyı bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            // 2. Zaten doğrulanmış mı?
            if (user.IsEmailVerified) return BadRequest("E-posta zaten doğrulanmış.");

            // 3. Kod doğru mu ve süresi dolmuş mu kontrol et
            if (user.EmailVerificationCode != request.Code || user.VerificationCodeExpiration < DateTime.UtcNow)
                return BadRequest("Geçersiz veya süresi dolmuş doğrulama kodu.");

            // 4. Doğrulamayı tamamla ve gereksiz kodları temizle
            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.VerificationCodeExpiration = null;

            await _context.SaveChangesAsync();

            return Ok("E-posta başarıyla doğrulandı. Artık sisteme giriş yapabilirsiniz.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Kullanıcıyı e-posta adresinden bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Kullanıcı yoksa veya şifre eşleşmiyorsa (BCrypt ile doğruluyoruz)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest("Hatalı e-posta veya şifre.");

            // 3. E-postasını doğrulamamışsa içeri alma
            if (!user.IsEmailVerified)
                return BadRequest("Lütfen giriş yapmadan önce e-posta adresinizi doğrulayın.");

            // 4. Şifre doğruysa JWT Token oluştur!
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token içine kullanıcının kimlik bilgilerini (Claims) gömüyoruz
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Sysadmin veya Client olduğu bilgisini gömüyoruz
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