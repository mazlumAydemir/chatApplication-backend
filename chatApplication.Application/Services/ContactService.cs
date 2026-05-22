using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

/// <summary>
/// Kullanıcı kontaklarını yönetme servisi
/// </summary>
public class ContactService : IContactService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Contact> _contactRepository;

    public ContactService(IGenericRepository<User> userRepository, IGenericRepository<Contact> contactRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
    }

    // ==========================================
    // REHBERE KULLANICI EKLE
    // ==========================================
    /// <summary>
    /// Rehbere yeni kullanıcı ekler
    /// </summary>
    public async Task AddContactAsync(int ownerId, string phoneNumber, string? savedName)
    {
        // 1. VALIDASYON
        if (ownerId <= 0)
            throw new ArgumentException("Geçerli bir kullanıcı ID'si gerekli.", nameof(ownerId));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Telefon numarası boş olamaz.", nameof(phoneNumber));

        // 2. EKLENMEK İSTENEN NUMARAYA SAHİP KULLANICI VAR MI?
        var users = await _userRepository.FindAsync(u => u.PhoneNumber == phoneNumber.Trim());
        var targetUser = users?.FirstOrDefault();

        if (targetUser == null)
            throw new InvalidOperationException("Bu numaraya ait bir kullanıcı bulunamadı.");

        // 3. KENDİNİ EKLEME KONTROLÜ
        if (targetUser.Id == ownerId)
            throw new InvalidOperationException("Kendinizi rehberinize ekleyemezsiniz.");

        // 4. ZATENKAYıTLı KONTROL
        var existingContacts = await _contactRepository.FindAsync(c =>
            c.OwnerId == ownerId && c.ContactUserId == targetUser.Id);

        if (existingContacts?.Any() == true)
            throw new InvalidOperationException("Bu kullanıcı zaten rehberinizde ekli.");

        // 5. REHBERe EKL
        var newContact = new Contact
        {
            OwnerId = ownerId,
            ContactUserId = targetUser.Id,
            SavedName = string.IsNullOrWhiteSpace(savedName)
                ? $"{targetUser.FirstName} {targetUser.LastName}".Trim()
                : savedName.Trim(),
            AddedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(newContact);
    }

    // ==========================================
    // REHBER LİSTESİNİ GETIR
    // ==========================================
    /// <summary>
    /// Kullanıcının rehberindeki tüm kişileri getirir
    /// </summary>
    public async Task<IEnumerable<object>> GetUserContactsAsync(int ownerId)
    {
        // 1. VALIDASYON
        if (ownerId <= 0)
            throw new ArgumentException("Geçerli bir kullanıcı ID'si gerekli.", nameof(ownerId));

        try
        {
            // 2. REHBERDEKİ TÜM KONTAKLARI GETIR
            var contacts = await _contactRepository.FindAsync(c => c.OwnerId == ownerId);
            var contactList = new List<object>();

            // 3. HER KONTAĞIN KULLANICI BİLGİSİNİ AL VE ÖZETİ HAZIRLA
            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    var targetUser = await _userRepository.GetByIdAsync(contact.ContactUserId);

                    if (targetUser != null)
                    {
                        contactList.Add(new
                        {
                            ContactId = targetUser.Id,
                            PhoneNumber = targetUser.PhoneNumber,
                            FirstName = targetUser.FirstName,
                            LastName = targetUser.LastName,
                            SavedName = contact.SavedName,
                            ProfilePictureUrl = targetUser.ProfilePictureUrl,
                            RsaPublicKey = targetUser.RsaPublicKey, // ✅ DÜZELTILDI: PublicKey → RsaPublicKey
                            IsOnline = targetUser.IsOnline,
                            LastSeen = targetUser.LastSeen,
                            AddedAt = contact.AddedAt
                        });
                    }
                }
            }

            return contactList;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Rehber getirilirken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // REHBERDEĞİ KULLANICIYI SİL
    // ==========================================
    /// <summary>
    /// Rehberden belirtilen kişiyi siler
    /// </summary>
    public async Task RemoveContactAsync(int ownerId, int contactUserId)
    {
        // 1. VALIDASYON
        if (ownerId <= 0 || contactUserId <= 0)
            throw new ArgumentException("Geçerli kullanıcı ID'leri gerekli.");

        try
        {
            // 2. KONTAĞI BUL
            var contacts = await _contactRepository.FindAsync(c =>
                c.OwnerId == ownerId && c.ContactUserId == contactUserId);

            var contact = contacts?.FirstOrDefault();

            if (contact == null)
                throw new InvalidOperationException("Bu kontakt rehberinizde bulunamadı.");

            // 3. SİL
            _contactRepository.Delete(contact);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Kontakt silinirken hata oluştu: {ex.Message}", ex);
        }
    }

    // ==========================================
    // REHBERDE KULLANICI ADINI GÜNCELLE
    // ==========================================
    /// <summary>
    /// Rehberde kaydedilen kullanıcı adını günceller
    /// </summary>
    public async Task UpdateContactNameAsync(int ownerId, int contactUserId, string newName)
    {
        // 1. VALIDASYON
        if (ownerId <= 0 || contactUserId <= 0)
            throw new ArgumentException("Geçerli kullanıcı ID'leri gerekli.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("İsim boş olamaz.", nameof(newName));

        try
        {
            // 2. KONTAĞI BUL
            var contacts = await _contactRepository.FindAsync(c =>
                c.OwnerId == ownerId && c.ContactUserId == contactUserId);

            var contact = contacts?.FirstOrDefault();

            if (contact == null)
                throw new InvalidOperationException("Bu kontakt rehberinizde bulunamadı.");

            // 3. İSMİ GÜNCELLE
            contact.SavedName = newName.Trim();
            _contactRepository.Update(contact);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Kontakt adı güncellenirken hata oluştu: {ex.Message}", ex);
        }
    }
}