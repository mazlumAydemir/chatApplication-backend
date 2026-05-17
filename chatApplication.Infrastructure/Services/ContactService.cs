using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Domain.Entities;
using chatApplication.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace chatApplication.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> AddContactAsync(Guid currentUserId, string contactEmail)
        {
            var contactUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == contactEmail);
            if (contactUser == null)
                throw new Exception("Bu e-posta adresine sahip bir kullanıcı bulunamadı.");

            if (contactUser.Id == currentUserId)
                throw new Exception("Kendinizi rehberinize ekleyemezsiniz.");

            var alreadyExists = await _context.Contacts
                .AnyAsync(c => c.UserId == currentUserId && c.ContactUserId == contactUser.Id);

            if (alreadyExists)
                throw new Exception("Bu kişi zaten rehberinizde ekli.");

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                ContactUserId = contactUser.Id
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return "Kişi başarıyla rehberinize eklendi.";
        }

        public async Task<List<UserDto>> GetContactsAsync(Guid currentUserId)
        {
            return await _context.Contacts
                .Include(c => c.ContactUser)
                .Where(c => c.UserId == currentUserId)
                .Select(c => new UserDto
                {
                    Id = c.ContactUser.Id,
                    FirstName = c.ContactUser.FirstName,
                    LastName = c.ContactUser.LastName,
                    PhoneNumber = c.ContactUser.PhoneNumber,
                    Role = c.ContactUser.Role.ToString()
                })
                .ToListAsync();
        }
    }
}