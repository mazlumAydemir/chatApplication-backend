using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Telefon numarası gerekli")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad gerekli")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Soyadı 50 karakterden fazla olamaz")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Şifre gerekli")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre onayı gerekli")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;
}