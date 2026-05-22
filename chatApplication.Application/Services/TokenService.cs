using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace chatApplication.Infrastructure.Services;

/// <summary>
/// JWT token oluşturma ve doğrulama servisi
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationDays;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // ✅ appsettings.json'dan ayarları oku
        _secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey yapılandırması eksik.");

        _issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer yapılandırması eksik.");

        _audience = _configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience yapılandırması eksik.");

        _expirationDays = int.TryParse(_configuration["JwtSettings:ExpirationDays"], out var days)
            ? days
            : 7; // Default 7 gün
    }

    // ==========================================
    // TOKEN OLUŞTUR
    // ==========================================
    /// <summary>
    /// Kullanıcı için JWT token oluşturur
    /// </summary>
    public string GenerateToken(User user)
    {
        // 1. VALIDASYON
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Kullanıcı bilgisi boş olamaz.");

        if (user.Id <= 0)
            throw new InvalidOperationException("Geçerli bir kullanıcı ID'si gerekli.");

        try
        {
            // 2. CLAIMS OLUŞTUR (Token içine gömülecek bilgiler)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role ?? "Client"),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("IssuedAt", DateTime.UtcNow.ToString("O"))
            };

            // 3. SIGNING KEY OLUŞTUR
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. TOKEN DESCRIPTOR OLUŞTUR
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_expirationDays),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds
            };

            // 5. TOKEN OLUŞTUR VE DÖN
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Token oluşturulurken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // TOKEN DOĞRULA
    // ==========================================
    /// <summary>
    /// JWT token'ı doğrular ve geçerliyse true döndürür
    /// </summary>
    public bool ValidateToken(string token)
    {
        // 1. VALIDASYON
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            // 2. TOKEN HANDLER OLUŞTUR
            var tokenHandler = new JwtSecurityTokenHandler();

            // 3. VALIDATION PARAMETRELERI AYARLA
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Token'ın tam vade sonunda expire olması
            };

            // 4. TOKEN'I DOĞRULA
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // 5. DOĞRULAMA BAŞARILI İSE TRUE DÖN
            return validatedToken != null;
        }
        catch (SecurityTokenExpiredException)
        {
            // Token süresi dolmuş
            return false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            // İmza geçersiz
            return false;
        }
        catch (SecurityTokenException)
        {
            // Diğer token hataları
            return false;
        }
        catch (Exception)
        {
            // Beklenmeyen hatalar
            return false;
        }
    }

    // ==========================================
    // TOKEN'DAN CLAIM ÇIKAR (İsteğe Bağlı - Gelecek Kullanım)
    // ==========================================
    /// <summary>
    /// Token'dan belirtilen claim'i çıkarır
    /// </summary>
    public string? GetClaimFromToken(string token, string claimType)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return null;

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);

            return claim?.Value;
        }
        catch
        {
            return null;
        }
    }
}