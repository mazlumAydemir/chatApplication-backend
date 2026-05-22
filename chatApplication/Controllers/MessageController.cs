using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace chatApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Sadece giriş yapmış kullanıcılar mesaj geçmişine erişebilir
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("history/{receiverId}")]
    public async Task<IActionResult> GetChatHistory(int receiverId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        try
        {
            // Token içerisinden istek atan mevcut kullanıcının ID'sini alıyoruz
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdStr, out int currentUserId))
                return Unauthorized(new { message = "Kullanıcı doğrulaması başarısız." });

            // İki kullanıcı arasındaki geçmişi servis katmanından çekiyoruz
            var history = await _messageService.GetChatHistoryAsync(currentUserId, receiverId);

            // Frontend tarafındaki skip/take (sayfalama) parametrelerine göre veriyi filtreleyip sıralıyoruz
            var paginatedHistory = history
                .OrderByDescending(m => m.SentAt) // Önce en yeni mesajları üste almak için (React scroll yukarı doğru çalıştığı için)
                .Skip(skip)
                .Take(take)
                .ToList();

            return Ok(paginatedHistory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}