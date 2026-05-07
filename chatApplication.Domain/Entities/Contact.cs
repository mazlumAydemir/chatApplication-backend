using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Rehberin Sahibi
        public Guid ContactUserId { get; set; } // Rehbere Eklenen Kişi
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigasyon özellikleri (İsteğe bağlı, Entity Framework için)
        public User User { get; set; }
        public User ContactUser { get; set; }
    }
}