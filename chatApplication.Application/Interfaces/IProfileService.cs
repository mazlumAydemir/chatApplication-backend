using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, UserProfileDto profileDto);
    }
}