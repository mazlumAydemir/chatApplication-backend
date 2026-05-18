using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Sadece giriş yapanlar erişebilir
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddContact([FromBody] AddContactDto request)
    {
        try
        {
            // Token'dan giriş yapan kişinin ID'sini alıyoruz
            var ownerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdStr, out int ownerId))
                return Unauthorized("Kullanıcı doğrulanamadı.");

            await _contactService.AddContactAsync(ownerId, request.PhoneNumber, request.SavedName);
            return Ok(new { message = "Kişi rehbere başarıyla eklendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetContacts()
    {
        try
        {
            var ownerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdStr, out int ownerId))
                return Unauthorized("Kullanıcı doğrulanamadı.");

            var contacts = await _contactService.GetUserContactsAsync(ownerId);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}