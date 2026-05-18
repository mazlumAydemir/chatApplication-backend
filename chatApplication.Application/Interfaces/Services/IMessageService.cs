using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Domain.Entities;

namespace chatApplication.Application.Interfaces.Services;

public interface IMessageService
{
    Task<Message> SaveMessageAsync(int senderId, int receiverId, string? textContent, string? encryptedImageUrl, string? encryptedSessionKey, string? digitalSignature);
    Task<IEnumerable<Message>> GetChatHistoryAsync(int user1Id, int user2Id);
}