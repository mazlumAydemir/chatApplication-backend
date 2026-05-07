using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace chatApplication.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MessageDto>> GetMessageHistoryAsync(Guid userId, Guid contactId, int skip = 0, int take = 20)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == contactId) ||
                            (m.SenderId == contactId && m.ReceiverId == userId))
                // DÜZELTME: Timestamp yerine senin veritabanındaki CreatedAt sütununu kullanıyoruz
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    EncryptedContent = m.EncryptedContent,
                    SenderEncryptedSessionKey = m.SenderEncryptedSessionKey,
                    ReceiverEncryptedSessionKey = m.ReceiverEncryptedSessionKey,
                    // DTO'nda tarih alanı yoksa buraya eklemene gerek yok. Varsa: CreatedAt = m.CreatedAt yazabilirsin.
                    IsRead = m.IsRead
                })
                .ToListAsync();

            messages.Reverse();
            return messages;
        }

        public async Task SaveMessageAsync(MessageDto dto)
        {
            var message = new chatApplication.Domain.Entities.Message
            {
                Id = Guid.NewGuid(),
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                EncryptedContent = dto.EncryptedContent,
                SenderEncryptedSessionKey = dto.SenderEncryptedSessionKey,
                ReceiverEncryptedSessionKey = dto.ReceiverEncryptedSessionKey,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}