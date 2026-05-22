using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Domain.Entities
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string EventType { get; set; } // "Login", "Error", "SecurityWarning"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
