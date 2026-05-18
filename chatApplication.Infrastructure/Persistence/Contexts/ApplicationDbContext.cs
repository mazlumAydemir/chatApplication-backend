using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using chatApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace chatApplication.Infrastructure.Persistence.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Veritabanı Tablolarımız
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Telefon Numarası Benzersiz (Unique) Olmalı
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // 2. Message -> Sender İlişkisi
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // Çakışmayı önlemek için Restrict

        // 3. Message -> Receiver İlişkisi
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // 4. Contact -> Owner (Rehberin Sahibi) İlişkisi
        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Owner)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // 5. Contact -> ContactUser (Rehbere Eklenen Kişi) İlişkisi
        modelBuilder.Entity<Contact>()
            .HasOne(c => c.ContactUser)
            .WithMany(u => u.ContactOf)
            .HasForeignKey(c => c.ContactUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}