using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs
{
    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty; // Yeni eklendi
        public string LastName { get; set; } = string.Empty;  // Yeni eklendi
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}