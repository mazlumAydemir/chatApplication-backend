using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.DTOs;

namespace chatApplication.Application.Interfaces.Services;

public interface IAdminService
{
    Task<SystemStatsDto> GetSystemStatsAsync();
}