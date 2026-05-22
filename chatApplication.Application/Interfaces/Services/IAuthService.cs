using chatApplication.Application.DTOs;
using chatApplication.Domain.Entities;
using System.Threading.Tasks;

namespace chatApplication.Application.Interfaces.Services;

public interface IAuthService
{
    /// <summary>
    /// Kullanıcı kaydını gerçekleştirir
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto request);

    /// <summary>
    /// Kullanıcı girişini gerçekleştirir
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto request);

    /// <summary>
    /// Kullanıcının RSA public key'ini günceller
    /// </summary>
    Task UpdatePublicKeyAsync(int userId, string publicKey);

    /// <summary>
    /// ID ile kullanıcı bilgilerini getirir
    /// </summary>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Refresh token ile yeni JWT token oluşturur
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Telefon numarası ile kullanıcı bulur
    /// </summary>
    Task<User?> GetUserByPhoneAsync(string phoneNumber);
}