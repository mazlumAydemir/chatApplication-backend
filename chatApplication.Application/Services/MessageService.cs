using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class MessageService : IMessageService
{
    private readonly IGenericRepository<Message> _messageRepository;

    public MessageService(IGenericRepository<Message> messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Message> SaveMessageAsync(int senderId, int receiverId, string? textContent, string? encryptedImageUrl, string? encryptedSessionKey, string? digitalSignature)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            TextContent = textContent,
            EncryptedImageUrl = encryptedImageUrl,
            EncryptedSessionKey = encryptedSessionKey,
            DigitalSignature = digitalSignature,
            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        return message;
    }

    public async Task<IEnumerable<Message>> GetChatHistoryAsync(int user1Id, int user2Id)
    {
        // İki kullanıcı arasındaki tüm geçmiş mesajları tarihe göre sıralı getirir
        var messages = await _messageRepository.FindAsync(m =>
            (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
            (m.SenderId == user2Id && m.ReceiverId == user1Id));

        return messages.OrderBy(m => m.SentAt).ToList();
    }
}