using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace chatApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Geçersiz veri gönderildi.", errors = ModelState });

            var result = await _authService.RegisterAsync(request);
            return Ok(new
            {
                message = "✅ Kayıt başarılı",
                token = result.Token,
                user = new { id = result.UserId, firstName = result.FirstName, lastName = result.LastName, phoneNumber = result.PhoneNumber }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"❌ Kayıt hatası: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Geçersiz veri gönderildi." });

            var result = await _authService.LoginAsync(request);
            return Ok(new
            {
                message = "✅ Giriş başarılı",
                token = result.Token,
                user = new { id = result.UserId, firstName = result.FirstName, lastName = result.LastName, phoneNumber = result.PhoneNumber, profilePictureUrl = result.ProfilePictureUrl }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = $"❌ Giriş başarısız: {ex.Message}" });
        }
    }

    // Chat.jsx şu an buraya istek atıyor, bu yüzden JsonElement koruması eklendi.
    [Authorize]
    [HttpPut("update-public-key")]
    public async Task<IActionResult> UpdatePublicKey([FromBody] JsonElement jsonElement)
    {
        try
        {
            string publicKey = jsonElement.ToString().Trim('"');
            if (string.IsNullOrWhiteSpace(publicKey))
                return BadRequest(new { message = "Public key boş olamaz." });

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            await _authService.UpdatePublicKeyAsync(userId, publicKey);
            return Ok(new { message = "✅ Genel şifreleme anahtarınız başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"❌ Hata: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("persistent")]
    public async Task<IActionResult> PersistentLogin()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                profilePictureUrl = user.ProfilePictureUrl,
                rsaPublicKey = user.RsaPublicKey, // User modelindeki RsaPublicKey kullanılıyor
                isOnline = user.IsOnline,
                message = "✅ Oturum aktif"
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = $"❌ Hata: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                profilePictureUrl = user.ProfilePictureUrl,
                isOnline = user.IsOnline,
                lastSeen = user.LastSeen
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"❌ Hata: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "✅ Çıkış başarılı" });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return BadRequest(new { message = "Refresh token gerekli." });

            var result = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(new { message = "✅ Token yenilendi", token = result.Token });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = $"❌ Token yenileme başarısız: {ex.Message}" });
        }
    }
    [Authorize]
    [HttpGet("user-info/{id}")]
    public async Task<IActionResult> GetUserInfo(int id)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                profilePictureUrl = user.ProfilePictureUrl,
                isOnline = user.IsOnline,
                lastSeen = user.LastSeen
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}