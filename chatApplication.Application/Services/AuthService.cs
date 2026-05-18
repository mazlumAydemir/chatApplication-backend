using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IGenericRepository<User> userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
    {
        // 1. Telefon numarası daha önce kullanılmış mı kontrol et
        var existingUsers = await _userRepository.FindAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (existingUsers.Any())
        {
            throw new Exception("Bu telefon numarası zaten kullanımda.");
        }

        // 2. Şifreyi BCrypt ile güvenli hale getir
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Yeni kullanıcıyı oluştur
        var newUser = new User
        {
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHash,
            PublicKey = request.PublicKey, // İleride RSA şifrelemesi için React'ten gelecek
            Role = "Client" // Varsayılan rol
        };

        await _userRepository.AddAsync(newUser);
        // Not: Gerçek senaryoda UnitOfWork kullanmıyorsak, IGenericRepository'e bir SaveChangesAsync() metodu eklememiz gerekir.
        // Şimdilik doğrudan context'e erişmediğimiz için bu noktada veritabanına kayıt tetiklenmeli.

        // 4. Token üret ve dön
        var token = _tokenService.GenerateToken(newUser);

        return new AuthResponseDto
        {
            UserId = newUser.Id,
            Token = token,
            Role = newUser.Role,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto request)
    {
        // 1. Kullanıcıyı telefon numarasından bul
        var users = await _userRepository.FindAsync(u => u.PhoneNumber == request.PhoneNumber);
        var user = users.FirstOrDefault();

        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        // 2. Şifreyi doğrula
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Hatalı şifre.");
        }

        // 3. Başarılıysa Token üret
        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Token = token,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}