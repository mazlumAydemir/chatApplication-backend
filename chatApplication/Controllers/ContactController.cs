using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddContact([FromBody] ContactAddDto dto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _contactService.AddContactAsync(currentUserId, dto.PhoneNumber);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetContacts()
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var contacts = await _contactService.GetContactsAsync(currentUserId);
            return Ok(contacts);
        }
    }
}