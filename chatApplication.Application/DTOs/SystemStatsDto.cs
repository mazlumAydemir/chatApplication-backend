using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace chatApplication.Application.DTOs;

public class SystemStatsDto
{
    public string ServerStatus { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalMessagesExchanged { get; set; }
    public int ClientUsers { get; set; }
    public int SysadminUsers { get; set; }
    public DateTime? LatestActivity { get; set; }
}