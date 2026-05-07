using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string EncryptedContent { get; set; } = string.Empty;

        // Geçmişi okuyabilmek için iki tarafa da kilitli anahtarlar lazım
        public string SenderEncryptedSessionKey { get; set; } = string.Empty;
        public string ReceiverEncryptedSessionKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}