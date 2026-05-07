using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.DTOs;

using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces
{
    public interface IMessageService
    {
        // Eski metodu silip bunu ekliyoruz (skip ve take eklendi, isim düzeltildi)
        Task<IEnumerable<MessageDto>> GetMessageHistoryAsync(Guid userId, Guid contactId, int skip = 0, int take = 20);

        Task SaveMessageAsync(MessageDto dto);
    }
}