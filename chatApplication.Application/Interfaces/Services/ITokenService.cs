using chatApplication.Domain.Entities;

namespace chatApplication.Application.Interfaces.Services;

/// <summary>
/// JWT Token oluşturma ve doğrulama servisi interface'i
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Kullanıcı için JWT token oluşturur
    /// </summary>
    /// <param name="user">Token oluşturulacak kullanıcı</param>
    /// <returns>JWT token string'i</returns>
    string GenerateToken(User user);

    /// <summary>
    /// JWT token'ı doğrular
    /// </summary>
    /// <param name="token">Doğrulanacak token</param>
    /// <returns>Geçerliyse true, değilse false</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Token'dan belirtilen claim'i çıkarır
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <param name="claimType">Çıkarılmak istenen claim tipi</param>
    /// <returns>Claim değeri veya null</returns>
    string? GetClaimFromToken(string token, string claimType);
}