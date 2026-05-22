using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token gerekli")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
