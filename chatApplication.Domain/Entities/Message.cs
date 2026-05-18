namespace chatApplication.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }

    // Hem resim hem text istendiği için
    public string? TextContent { get; set; }

    // DES ile şifrelenmiş resmin yolu (Zorunlu İster)
    public string? EncryptedImageUrl { get; set; }

    // Görüntüyü çözecek DES anahtarının, alıcının RSA public key'i ile şifrelenmiş hali (Zorunlu İster)
    public string? EncryptedSessionKey { get; set; }

    // Resmin kaynağını doğrulamak için göndericinin imzaladığı veri (Zorunlu İster)
    public string? DigitalSignature { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // EF Core Relational Properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}