using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class ContactService : IContactService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Contact> _contactRepository;

    public ContactService(IGenericRepository<User> userRepository, IGenericRepository<Contact> contactRepository)
    {
        _userRepository = userRepository;
        _contactRepository = contactRepository;
    }

    public async Task AddContactAsync(int ownerId, string phoneNumber, string? savedName)
    {
        // 1. Eklenmek istenen numaraya sahip bir kullanıcı var mı?
        var users = await _userRepository.FindAsync(u => u.PhoneNumber == phoneNumber);
        var targetUser = users.FirstOrDefault();

        if (targetUser == null)
            throw new Exception("Bu numaraya ait bir kullanıcı bulunamadı.");

        if (targetUser.Id == ownerId)
            throw new Exception("Kendinizi rehberinize ekleyemezsiniz.");

        // 2. Bu kişi zaten rehberde var mı?
        var existingContacts = await _contactRepository.FindAsync(c => c.OwnerId == ownerId && c.ContactUserId == targetUser.Id);
        if (existingContacts.Any())
            throw new Exception("Bu kullanıcı zaten rehberinizde ekli.");

        // 3. Rehbere Ekle
        var newContact = new Contact
        {
            OwnerId = ownerId,
            ContactUserId = targetUser.Id,
            SavedName = string.IsNullOrWhiteSpace(savedName) ? $"{targetUser.FirstName} {targetUser.LastName}" : savedName,
            AddedAt = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(newContact);
    }

    public async Task<IEnumerable<object>> GetUserContactsAsync(int ownerId)
    {
        // Kişinin rehberindeki listeyi getirir
        var contacts = await _contactRepository.FindAsync(c => c.OwnerId == ownerId);

        var contactList = new List<object>();
        foreach (var contact in contacts)
        {
            var targetUser = await _userRepository.GetByIdAsync(contact.ContactUserId);
            if (targetUser != null)
            {
                contactList.Add(new
                {
                    ContactId = targetUser.Id,
                    PhoneNumber = targetUser.PhoneNumber,
                    SavedName = contact.SavedName,
                    PublicKey = targetUser.PublicKey // Şifreli mesajlaşma için React'e lazım olacak
                });
            }
        }
        return contactList;
    }
}