using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Domain.Entities;

public class Contact
{
    public int Id { get; set; }

    // Rehberin asıl sahibi olan kullanıcı
    public int OwnerId { get; set; }

    // Rehbere eklenen kullanıcının ID'si
    public int ContactUserId { get; set; }

    // Kullanıcının bu kişiyi rehberine kaydettiği özel isim (Opsiyonel)
    public string? SavedName { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // EF Core Relational Properties
    public User Owner { get; set; } = null!;
    public User ContactUser { get; set; } = null!;
}