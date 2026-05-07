using chatApplication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int OnlineUsers { get; set; }
    public int TotalMessages { get; set; }
    public List<UserDto> RecentUsers { get; set; } = new();
}