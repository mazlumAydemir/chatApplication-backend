using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace chatApplication.Application.Services;

public class MessageService : IMessageService
{
    private readonly IGenericRepository<Message> _messageRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IGenericRepository<Message> messageRepository,
        IGenericRepository<User> userRepository,
        ILogger<MessageService> logger
    )
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    // ==========================================
    // 1. MESAJ KAYDETME (Legacy - Opsiyonel)
    // ==========================================
    /// <summary>
    /// ✅ DÜZELTILMIŞ: SenderEncryptedSessionKey parametresi EKLENDİ
    /// Tüm parametreleri alarak Message objesini oluşturur ve database'e kaydeder
    /// </summary>
    public async Task<Message> SaveMessageAsync(
        int senderId,
        int receiverId,
        string? textContent,
        string? encryptedImageUrl,
        string? encryptedSessionKey,
        string? senderEncryptedSessionKey,  // ✅ EKLENDI
        string? digitalSignature
    )
    {
        try
        {
            // ==========================================
            // GÜVENLIK KONTROLLERI
            // ==========================================

            // 1. Token'daki kullanıcı var mı?
            var sender = await _userRepository.GetByIdAsync(senderId);
            if (sender == null)
            {
                var errorMsg = $"Kritik Hata: Token'daki ID ({senderId}) veritabanında yok! Lütfen React uygulamasından ÇIKIŞ YAPIP tekrar giriş yapın.";
                _logger.LogError($"❌ {errorMsg}");
                throw new Exception(errorMsg);
            }

            // 2. Alıcı var mı?
            var receiver = await _userRepository.GetByIdAsync(receiverId);
            if (receiver == null)
            {
                var errorMsg = "Kritik Hata: Mesaj göndermeye çalıştığınız kullanıcı artık veritabanında mevcut değil.";
                _logger.LogError($"❌ {errorMsg}");
                throw new Exception(errorMsg);
            }

            // 3. En az TextContent veya EncryptedImageUrl olmalı
            if (string.IsNullOrWhiteSpace(textContent) && string.IsNullOrWhiteSpace(encryptedImageUrl))
            {
                throw new ArgumentException("Mesaj içeriği (TextContent veya EncryptedImageUrl) boş olamaz!");
            }

            // ==========================================
            // MESSAGE OBJESINI OLUŞTUR
            // ==========================================
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                TextContent = textContent,
                EncryptedImageUrl = encryptedImageUrl,
                EncryptedSessionKey = encryptedSessionKey,
                SenderEncryptedSessionKey = senderEncryptedSessionKey,  // ✅ EKLENDI
                DigitalSignature = digitalSignature,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            // ==========================================
            // DATABASE'E KAYDET
            // ==========================================
            await _messageRepository.AddAsync(message);
            _logger.LogInformation($"✅ Mesaj kaydedildi: {senderId} → {receiverId}");

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ SaveMessageAsync Hatası: {ex.Message}");
            throw;
        }
    }

    // ==========================================
    // 2. DOĞRUDAN MESSAGE OBJESINI KAYDET
    // ==========================================
    /// <summary>
    /// ✅ EKLENDI: Direct Message objesini database'e ekler
    /// ChatHub.cs'de kullanılıyor
    /// </summary>
    public async Task AddMessageAsync(Message message)
    {
        try
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message obje null olamaz!");
            }

            // ==========================================
            // GÜVENLIK KONTROLLERI
            // ==========================================

            // Sender var mı?
            var sender = await _userRepository.GetByIdAsync(message.SenderId);
            if (sender == null)
            {
                var errorMsg = $"Sender ID {message.SenderId} veritabanında bulunamadı!";
                _logger.LogError($"❌ {errorMsg}");
                throw new Exception(errorMsg);
            }

            // Receiver var mı?
            var receiver = await _userRepository.GetByIdAsync(message.ReceiverId);
            if (receiver == null)
            {
                var errorMsg = $"Receiver ID {message.ReceiverId} veritabanında bulunamadı!";
                _logger.LogError($"❌ {errorMsg}");
                throw new Exception(errorMsg);
            }

            // En az bir içerik var mı?
            if (string.IsNullOrWhiteSpace(message.TextContent) &&
                string.IsNullOrWhiteSpace(message.EncryptedImageUrl))
            {
                throw new ArgumentException("Mesaj içeriği (TextContent veya EncryptedImageUrl) boş olamaz!");
            }

            // ==========================================
            // TIMESTAMP KONTROL
            // ==========================================
            if (message.SentAt == default(DateTime))
            {
                message.SentAt = DateTime.UtcNow;
            }

            message.IsRead = false;

            // ==========================================
            // DATABASE'E KAYDET
            // ==========================================
            await _messageRepository.AddAsync(message);
            _logger.LogInformation($"✅ Mesaj kaydedildi (AddMessageAsync): {message.SenderId} → {message.ReceiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ AddMessageAsync Hatası: {ex.Message}");
            throw;
        }
    }

    // ==========================================
    // 3. MESAJLARI OKUNDU OLARAK İŞARETLE
    // ==========================================
    /// <summary>
    /// ✅ EKLENDI: SenderId tarafından gönderilen mesajları readerId tarafından okunmuş olarak işaretler
    /// Parametre sırası: SenderId (gönderici), ReaderId (okuyan/alıcı)
    /// </summary>
    public async Task MarkMessagesAsReadAsync(int senderId, int readerId)
    {
        try
        {
            // Gönderici ve okuyan kişi same person olamaz
            if (senderId == readerId)
            {
                _logger.LogWarning($"⚠️ MarkMessagesAsReadAsync: Gönderici ve okuyan aynı kişi (ID: {senderId})");
                return;
            }

            // senderId tarafından gönderilen ve readerId'ye ulaşan tüm okunmamış mesajları bul
            var messagesToMark = await _messageRepository.FindAsync(m =>
                m.SenderId == senderId &&
                m.ReceiverId == readerId &&
                !m.IsRead
            );

            if (!messagesToMark.Any())
            {
                _logger.LogInformation($"ℹ️ Okunmamış mesaj yok: {senderId} → {readerId}");
                return;
            }

            // Mesajları okundu olarak işaretle
            var now = DateTime.UtcNow;
            foreach (var message in messagesToMark)
            {
                message.IsRead = true;
                message.ReadAt = now;
                await _messageRepository.UpdateAsync(message);
            }

            _logger.LogInformation($"✅ {messagesToMark.Count()} mesaj okundu olarak işaretlendi: {senderId} → {readerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ MarkMessagesAsReadAsync Hatası: {ex.Message}");
            throw;
        }
    }

    // ==========================================
    // 4. SOHBET GEÇMİŞİ GETIR
    // ==========================================
    /// <summary>
    /// İki kullanıcı arasındaki tüm mesajları gönderim zamanına göre sıralı olarak getirir
    /// </summary>
    public async Task<IEnumerable<Message>> GetChatHistoryAsync(int user1Id, int user2Id)
    {
        try
        {
            // user1Id ve user2Id same olamaz
            if (user1Id == user2Id)
            {
                _logger.LogWarning($"⚠️ GetChatHistoryAsync: İki user ID aynı (ID: {user1Id})");
                return Enumerable.Empty<Message>();
            }

            // Her iki yönde iletişim olan mesajları bul
            var messages = await _messageRepository.FindAsync(m =>
                (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                (m.SenderId == user2Id && m.ReceiverId == user1Id)
            );

            var sortedMessages = messages
                .OrderBy(m => m.SentAt)
                .ToList();

            _logger.LogInformation($"✅ Sohbet geçmişi alındı: {user1Id} ↔ {user2Id} ({sortedMessages.Count} mesaj)");

            return sortedMessages;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetChatHistoryAsync Hatası: {ex.Message}");
            throw;
        }
    }
}