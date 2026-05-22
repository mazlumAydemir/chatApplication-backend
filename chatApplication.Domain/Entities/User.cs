using System;
using System.Collections.Generic;

namespace chatApplication.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // ✅ RSA Public Key (standart isim)
    public string? RsaPublicKey { get; set; }
    public DateTime? RsaKeyUpdatedAt { get; set; }

    public string Role { get; set; } = "Client";

    // Online Status
    public bool IsOnline { get; set; } = false;
    public DateTime? LastSeen { get; set; }

    // Navigation Properties
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Contact> ContactOf { get; set; } = new List<Contact>();
}