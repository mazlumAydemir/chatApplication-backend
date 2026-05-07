using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using chatApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("history/{contactId}")]
        public async Task<IActionResult> GetHistory(Guid contactId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var messages = await _messageService.GetMessageHistoryAsync(userId, contactId, skip, take);
            return Ok(messages);
        }
    }
}