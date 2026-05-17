using System;

namespace chatApplication.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        // Giriş işlemi için ana kimlik alanı artık telefon numarası (Zorunlu yapıldı)
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // --- PROFİL ALANLARI ---
        // Email artık zorunlu değil, isteğe bağlı bir profil alanı haline getirildi
     
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
    

        // Kullanıcı Rolü (Enum türünden)
        public chatApplication.Domain.Enums.UserRole Role { get; set; }

        // Durum ve Zaman Bilgileri
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}