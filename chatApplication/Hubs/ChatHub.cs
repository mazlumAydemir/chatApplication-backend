using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace chatApplication.Hubs;

[Authorize] // Sadece giriş yapmış (Token'ı olan) kullanıcılar bağlanabilir
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    // React tarafından bu metot çağrılacak
    public async Task SendMessage(int receiverId, string? textContent, string? encryptedImageUrl, string? encryptedSessionKey, string? digitalSignature)
    {
        // 1. Gönderenin ID'sini Token'dan alıyoruz
        var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(senderIdStr, out int senderId))
        {
            throw new HubException("Kullanıcı kimliği doğrulanamadı.");
        }

        // 2. Mesajı Veritabanına Kaydet
        var savedMessage = await _messageService.SaveMessageAsync(senderId, receiverId, textContent, encryptedImageUrl, encryptedSessionKey, digitalSignature);

        // 3. Mesajı Alıcıya (Eğer o an online ise) Anlık Olarak İlet
        // SignalR, Program.cs'deki CustomUserIdProvider sayesinde "receiverId"nin hangi WebSocket bağlantısı olduğunu bilir.
        await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
        {
            MessageId = savedMessage.Id,
            SenderId = senderId,
            TextContent = textContent,
            EncryptedImageUrl = encryptedImageUrl,
            EncryptedSessionKey = encryptedSessionKey,
            DigitalSignature = digitalSignature,
            SentAt = savedMessage.SentAt
        });
    }

    // Kullanıcı bağlandığında (Opsiyonel olarak loglama veya Redis'e online durumunu yazmak için kullanılabilir)
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"Kullanıcı Bağlandı: {userId} | ConnectionId: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    // Kullanıcı koptuğunda
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"Kullanıcı Koptu: {userId}");

        await base.OnDisconnectedAsync(exception);
    }
}