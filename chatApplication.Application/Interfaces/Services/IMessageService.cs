using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Interfaces.Services;

public interface IMessageService
{
    // ==========================================
    // MESAJ KAYDETME
    // ==========================================

    /// <summary>
    /// ✅ DÜZELTILMIŞ: SenderEncryptedSessionKey parametresi EKLENDİ
    /// Şifreli mesajı database'e kaydeder
    /// </summary>
    Task<Message> SaveMessageAsync(
        int senderId,
        int receiverId,
        string? textContent,
        string? encryptedImageUrl,
        string? encryptedSessionKey,
        string? senderEncryptedSessionKey,  // ✅ EKLENDI
        string? digitalSignature
    );

    /// <summary>
    /// Direct Message objesini database'e ekler
    /// ChatHub'da kullanılıyor
    /// </summary>
    Task AddMessageAsync(Message message);

    // ==========================================
    // MESAJ OKUMA
    // ==========================================

    /// <summary>
    /// ✅ EKLENDI: Mesajları okundu olarak işaretler
    /// ChatHub.cs'de MarkMessagesAsRead endpoint'inde çağrılıyor
    /// </summary>
    Task MarkMessagesAsReadAsync(int senderId, int readerId);

    /// <summary>
    /// İki kullanıcı arasındaki sohbet geçmişini getirir
    /// Sohbet alanında kullanılıyor (history yükleme)
    /// </summary>
    Task<IEnumerable<Message>> GetChatHistoryAsync(int user1Id, int user2Id);
}