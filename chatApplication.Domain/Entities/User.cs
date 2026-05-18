using System;


namespace chatApplication.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    public string? PublicKey { get; set; }
    public string Role { get; set; } = "Client";

    // EF Core Relational Properties (Navigation)
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

    // Kullanıcının kendi rehberindeki kişiler
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    // Bu kullanıcıyı rehberine ekleyenler (EF Core ters ilişkisi için faydalı)
    public ICollection<Contact> ContactOf { get; set; } = new List<Contact>();
}