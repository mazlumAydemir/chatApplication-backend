using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs;

public class AddContactDto
{
    // Eklenecek kişinin numarası
    public string PhoneNumber { get; set; } = string.Empty;

    // Rehbere hangi isimle kaydedileceği (Opsiyonel)
    public string? SavedName { get; set; }
}