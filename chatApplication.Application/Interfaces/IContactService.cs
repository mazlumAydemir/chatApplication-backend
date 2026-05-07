using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces
{
    public interface IContactService
    {
        Task<string> AddContactAsync(Guid currentUserId, string contactEmail);
        Task<List<UserDto>> GetContactsAsync(Guid currentUserId);
    }
}