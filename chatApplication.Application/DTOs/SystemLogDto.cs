using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs;

public class SystemLogDto
{
    // Logun türü (örn: "Login", "Error", "Security")
    public string EventType { get; set; }

    // Logun açıklaması
    public string Message { get; set; }

    // Logun oluşma zamanı
    public DateTime Timestamp { get; set; }
}