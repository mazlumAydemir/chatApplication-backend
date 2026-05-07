namespace chatApplication.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // --- YENİ EKLENEN ZENGİN PROFİL ALANLARI ---
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        // -------------------------------------------

        public bool IsEmailVerified { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? VerificationCodeExpiration { get; set; }

        // İŞTE DÜZELTTİĞİMİZ SATIR (String yerine senin projendeki Enum'u kullanıyoruz)
        public chatApplication.Domain.Enums.UserRole Role { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}