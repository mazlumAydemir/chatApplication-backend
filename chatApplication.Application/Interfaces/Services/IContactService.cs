using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.Interfaces.Services;

public interface IContactService
{
    Task AddContactAsync(int ownerId, string phoneNumber, string? savedName);
    Task<IEnumerable<object>> GetUserContactsAsync(int ownerId);
}