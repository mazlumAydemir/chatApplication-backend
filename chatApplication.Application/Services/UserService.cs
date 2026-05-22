using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;

namespace chatApplication.Application.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task UpdateUserStatusAsync(int userId, bool isOnline)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user != null)
        {
            user.IsOnline = isOnline;

            if (!isOnline)
            {
                user.LastSeen = DateTime.UtcNow;
            }

            // Arayüzünde 'Update' metodu senkron olduğu için 'await' kullanma
            _userRepository.Update(user);
        }
    }
}