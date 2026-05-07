using chatApplication.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bu sınıfa sadece JWT token'ı olan (giriş yapmış) kullanıcılar erişebilir!
    public class RsaKeyController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public RsaKeyController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpPost("upload-public-key")]
        public async Task<IActionResult> UploadPublicKey([FromBody] RsaPublicKeyDto request)
        {
            // Token'dan kullanıcının ID'sini çekiyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Yönergeye göre anahtarlar periyodik güncelleniyor. Biz de Redis'te 24 saat ömür veriyoruz.
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            // Kullanıcının Public Key'ini Redis'e kaydediyoruz (Key: RsaKey_kullaniciId)
            await _cache.SetStringAsync($"RsaKey_{userId}", request.PublicKey, options);

            return Ok("RSA Public Key başarıyla sunucuya (Redis) yüklendi.");
        }

        [HttpGet("get-public-key/{targetUserId}")]
        public async Task<IActionResult> GetPublicKey(string targetUserId)
        {
            // Sohbet etmek istediğimiz kişinin Public Key'ini Redis'ten çekiyoruz
            var publicKey = await _cache.GetStringAsync($"RsaKey_{targetUserId}");

            if (string.IsNullOrEmpty(publicKey))
                return NotFound("Kullanıcının güncel bir RSA Public Key'i bulunamadı. Çevrimdışı olabilir.");

            return Ok(new { TargetUserId = targetUserId, PublicKey = publicKey });
        }
    }
}