using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Telefon numarası gerekli")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gerekli")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string Password { get; set; } = string.Empty;
}