namespace chatApplication.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    // ==========================================
    // SENDER & RECEIVER
    // ==========================================
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }

    // ==========================================
    // MESAJ İÇERİĞİ (Metin veya Resim)
    // ==========================================
    /// <summary>
    /// Metin mesajı (şifresiz veya şifreli olabilir)
    /// Örnek: "Merhaba!" veya "[GALLERY]url1|url2|url3"
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Şifreli resim URL'si (/uploads/dosya.enc)
    /// Eğer birden fazla resim varsa pipe ile ayrılır: /uploads/img1.enc|/uploads/img2.enc
    /// </summary>
    public string? EncryptedImageUrl { get; set; }

    // ==========================================
    // ŞİFRELEME AYARLARI (Alıcı için)
    // ==========================================
    /// <summary>
    /// DES Oturum Anahtarının, ALICININ RSA Public Key'i ile şifrelenmiş hali
    /// Alıcı kendi Private Key'i ile çözecek
    /// </summary>
    public string? EncryptedSessionKey { get; set; }

    /// <summary>
    /// DES Oturum Anahtarının, GÖNDERİCİNİN RSA Public Key'i ile şifrelenmiş hali
    /// Gönderici kendi private key'i ile çözebilir
    /// </summary>
    public string? SenderEncryptedSessionKey { get; set; }

    /// <summary>
    /// Mesajın orijinalliğini doğrulayan imza
    /// Gönderici tarafından EncryptedContent'in RSA Private Key'i ile imzalanması
    /// </summary>
    public string? DigitalSignature { get; set; }

    // ==========================================
    // DURUM ALANLARI
    // ==========================================
    /// <summary>
    /// Mesaj okundu mu?
    /// ✅ true = Okundu
    /// ❌ false = Okunmadı
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Mesaj ne zaman okundu?
    /// null = Hala okunmadı
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Mesaj ne zaman gönderildi?
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // ==========================================
    // ENTITY FRAMEWORK RELATIONAL PROPERTIES
    // ==========================================
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}