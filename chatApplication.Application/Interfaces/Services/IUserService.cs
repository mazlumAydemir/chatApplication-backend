using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace chatApplication.Application.Interfaces.Services;

public interface IUserService
{
    Task UpdateUserStatusAsync(int userId, bool isOnline);
}