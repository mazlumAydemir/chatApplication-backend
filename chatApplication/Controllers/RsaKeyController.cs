using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace chatApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RsaKeyController : ControllerBase
{
    private readonly IGenericRepository<User> _userRepository;

    public RsaKeyController(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    // ==========================================
    // 1. PUBLIC KEY GETIR
    // ==========================================
    /// <summary>
    /// Belirtilen kullanıcının RSA public key'ini getirir
    /// </summary>
    [HttpGet("get-public-key/{userId}")]
    public async Task<IActionResult> GetPublicKey(int userId)
    {
        try
        {
            if (userId <= 0)
                return BadRequest(new { message = "Geçerli bir kullanıcı ID'si gerekli." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            // User modelindeki yeni isim: RsaPublicKey
            if (string.IsNullOrEmpty(user.RsaPublicKey))
                return BadRequest(new { message = "Bu kullanıcının henüz oluşturulmuş bir şifreleme anahtarı yok." });

            return Ok(new
            {
                publicKey = user.RsaPublicKey,
                updatedAt = user.RsaKeyUpdatedAt,
                message = "✅ Public key başarıyla getirildi"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================
    // 2. PUBLIC KEY GÜNCELLE (JsonElement Korumalı)
    // ==========================================
    /// <summary>
    /// Kendi RSA public key'ini günceller
    /// </summary>
    [HttpPut("update-public-key")]
    public async Task<IActionResult> UpdatePublicKey([FromBody] JsonElement jsonElement)
    {
        try
        {
            // React'tan gelen JSON içindeki string veriyi güvenli bir şekilde tırnaklardan arındırıyoruz
            string publicKey = jsonElement.ToString().Trim('"');

            if (string.IsNullOrWhiteSpace(publicKey))
                return BadRequest(new { message = "Public key boş olamaz." });

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            user.RsaPublicKey = publicKey;
            user.RsaKeyUpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ Public key başarıyla güncellendi",
                updatedAt = user.RsaKeyUpdatedAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Hata: {ex.Message}" });
        }
    }

    // ==========================================
    // 3. PUBLIC KEY SİL 
    // ==========================================
    /// <summary>
    /// Kendi RSA public key'ini siler (şifreleme devre dışı bırakma)
    /// </summary>
    [HttpDelete("delete-public-key")]
    public async Task<IActionResult> DeletePublicKey()
    {
        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            user.RsaPublicKey = null;
            user.RsaKeyUpdatedAt = null;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { message = "✅ Public key başarıyla silindi" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Hata: {ex.Message}" });
        }
    }
}