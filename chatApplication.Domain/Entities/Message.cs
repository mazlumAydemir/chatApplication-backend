using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; } // Gönderen
        public Guid ReceiverId { get; set; } // Alan

        // Şifreli Mesajın Kendisi (AES ile şifrelenmiş)
        public string EncryptedContent { get; set; } = string.Empty;

        // Karşı tarafın okuyabilmesi için onun Public Key'i ile şifrelenmiş AES Anahtarı
        public string ReceiverEncryptedSessionKey { get; set; } = string.Empty;

        // Senin kendi mesajını okuyabilmen için senin Public Key'in ile şifrelenmiş AES Anahtarı
        public string SenderEncryptedSessionKey { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}