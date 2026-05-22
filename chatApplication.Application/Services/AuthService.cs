using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.Services;

/// <summary>
/// Kimlik doğrulama (Authentication) işlemlerini gerçekleştiren servis
/// </summary>
public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IGenericRepository<User> userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    // ==========================================
    // 1. KAYIT (REGISTER)
    // ==========================================
    /// <summary>
    /// Yeni kullanıcı kaydını gerçekleştirir
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
    {
        // 1. INPUT VALIDASYONU
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Kayıt verisi boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new InvalidOperationException("Telefon numarası boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new InvalidOperationException("Ad boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new InvalidOperationException("Şifre en az 6 karakter olmalıdır.");

        if (request.Password != request.ConfirmPassword)
            throw new InvalidOperationException("Şifreler eşleşmiyor.");

        // 2. TELEFON NUMARASI ZATENKAYıTLı Mı?
        var existingUser = await GetUserByPhoneAsync(request.PhoneNumber.Trim());
        if (existingUser != null)
            throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı. Lütfen farklı bir numara kullanın.");

        // 3. YENİ KULLANICI OLUŞTUR
        try
        {
            var user = new User
            {
                PhoneNumber = request.PhoneNumber.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName?.Trim() ?? string.Empty,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Client",
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                RsaPublicKey = null,
                RsaKeyUpdatedAt = null
            };

            // 4. VERİTABANıNA KAYDET
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // 5. TOKEN OLUŞTUR
            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Kayıt sırasında bir hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // 2. GİRİŞ (LOGIN)
    // ==========================================
    /// <summary>
    /// Kullanıcı girişini gerçekleştirir
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto request)
    {
        // 1. INPUT VALIDASYONU
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Giriş verisi boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new InvalidOperationException("Telefon numarası boş olamaz.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Şifre boş olamaz.");

        // 2. KULLANICIYI TELEFON NUMARASI İLE BUL
        var user = await GetUserByPhoneAsync(request.PhoneNumber.Trim());
        if (user == null)
            throw new InvalidOperationException("Telefon numarası veya şifre yanlış.");

        // 3. ŞİFRESİ DOĞRULA
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidOperationException("Telefon numarası veya şifre yanlış.");

        // 4. KULLANICIYI ÇEVRİMİÇİ İŞARETLE
        try
        {
            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Kullanıcı durumu güncellenirken hata oluştu: {ex.Message}", ex);
        }

        // 5. TOKEN OLUŞTUR
        try
        {
            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.LastSeen
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Token oluşturulurken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // 3. PUBLIC KEY GÜNCELLEME
    // ==========================================
    /// <summary>
    /// Kullanıcının RSA public key'ini günceller (end-to-end şifreleme için)
    /// </summary>
    public async Task UpdatePublicKeyAsync(int userId, string publicKey)
    {
        // 1. VALIDASYON
        if (userId <= 0)
            throw new ArgumentException("Geçerli bir kullanıcı ID'si gerekli.", nameof(userId));

        if (string.IsNullOrWhiteSpace(publicKey))
            throw new ArgumentException("Public key boş olamaz.", nameof(publicKey));

        // 2. KULLANICIYI BUL
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"ID {userId} olan kullanıcı bulunamadı.");

        // 3. PUBLIC KEY'İ GÜNCELLE
        try
        {
            user.RsaPublicKey = publicKey.Trim();
            user.RsaKeyUpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Public key güncellenirken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // 4. ID İLE KULLANICI GETIR
    // ==========================================
    /// <summary>
    /// Kullanıcı ID'sine göre kullanıcı bilgilerini getirir
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
            return null;

        try
        {
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Kullanıcı aranırken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // 5. TELEFON NUMARASI İLE KULLANICI GETIR
    // ==========================================
    /// <summary>
    /// Telefon numarasına göre kullanıcı bulur
    /// </summary>
    public async Task<User?> GetUserByPhoneAsync(string phoneNumber)
    {
        // 1. VALIDASYON
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        try
        {
            // 2. VERİTABANıNDA ARA
            var users = await _userRepository.FindAsync(u =>
                u.PhoneNumber == phoneNumber.Trim());

            // 3. İLK SONUCU DÖN
            return users?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Telefon numarasına göre kullanıcı aranırken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // 6. REFRESH TOKEN
    // ==========================================
    /// <summary>
    /// Süresi dolan JWT token'ı yeniler (Future implementation)
    /// </summary>
    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // NOT: Bu method, refresh token sistemi kurulduktan sonra doldurulacaktır.
        // Şu anda dummy implementasyon.

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token gerekli.", nameof(refreshToken));

        try
        {
            // TODO: Refresh token'ı Redis'ten kontrol et
            // TODO: Eski token'ın payload'ını decode et
            // TODO: Yeni token'ı oluştur ve geri döndür

            throw new NotImplementedException("Refresh token sistemi henüz uygulanmadı.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Token yenileme başarısız: {ex.Message}", ex);
        }
    }
}