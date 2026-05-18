using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Domain.Entities;

namespace chatApplication.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}