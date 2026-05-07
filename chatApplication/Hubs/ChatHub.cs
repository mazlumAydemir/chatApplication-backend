using chatApplication.Application.DTOs;
using chatApplication.Application.Interfaces;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace chatApplication.Hubs
{
    [Authorize] // Sadece giriş yapmış kullanıcılar bağlanabilsin
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly ApplicationDbContext _context;
        public static int ConnectedUserCount = 0;

        // Hangi kullanıcının hangi Connection ID ile bağlı olduğunu tutuyoruz
        private static readonly Dictionary<Guid, string> _connectedUsers = new();

        // 1. HATA ÇÖZÜMÜ: İki servisi de TEK BİR constructor (yapıcı metot) içinde alıyoruz
        public ChatHub(ApplicationDbContext context, IMessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        // 2. HATA ÇÖZÜMÜ: İki farklı OnConnectedAsync metodunu BİRLEŞTİRDİK
        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                // A) Kullanıcıyı SignalR hafızasına ekle
                _connectedUsers[userId] = Context.ConnectionId;

                // B) Veritabanında Çevrimiçi durumunu güncelle
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Diğer kullanıcılara bildir
                    await Clients.All.SendAsync("UserStatusChanged", userId.ToString(), true, user.LastSeen);
                }
            }

            await base.OnConnectedAsync();
        }

        // 2. HATA ÇÖZÜMÜ: İki farklı OnDisconnectedAsync metodunu BİRLEŞTİRDİK
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                // A) Kullanıcıyı SignalR hafızasından sil
                _connectedUsers.Remove(userId);

                // B) Veritabanında Çevrimdışı durumunu güncelle
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Diğer kullanıcılara bildir
                    await Clients.All.SendAsync("UserStatusChanged", userId.ToString(), false, user.LastSeen);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // React'ten fırlatılan şifreli paketi karşılayan metot
        public async Task SendEncryptedPayload(Guid receiverId, string payloadJson)
        {
            var senderIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(senderIdStr, out Guid senderId)) return;

            // 1. Gelen JSON verisini ayrıştırıyoruz
            var payload = JsonSerializer.Deserialize<JsonElement>(payloadJson);

            // React tarafından gönderilecek olan Çift Kilitli verileri alıyoruz
            var encryptedData = payload.GetProperty("encryptedData").GetString()!;
            var receiverKey = payload.GetProperty("encryptedSessionKey").GetString()!;

            // Eğer React tarafı kendi kilidini göndermediyse boş geçiyoruz (çökme olmasın diye)
            var senderKey = payload.TryGetProperty("senderEncryptedSessionKey", out var sKey) ? sKey.GetString()! : string.Empty;

            // 2. Veritabanına kaydedilmesi için DTO oluşturup Servis'e gönderiyoruz
            var messageDto = new MessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                EncryptedContent = encryptedData,
                ReceiverEncryptedSessionKey = receiverKey,
                SenderEncryptedSessionKey = senderKey
            };

            await _messageService.SaveMessageAsync(messageDto);

            // 3. Eğer alıcı kişi o an aktif (Online) ise mesajı canlı olarak onun ekranına fırlat
            if (_connectedUsers.TryGetValue(receiverId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceivePayload", payloadJson);
            }
        }

        // Yazıyor... durumunu gönderen metot
        public async Task SendTyping(string receiverId, bool isTyping)
        {
            var senderIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(senderIdStr))
            {
                await Clients.User(receiverId).SendAsync("ReceiveTyping", senderIdStr, isTyping);
            }
        }
        // ========================================================
        // WEBRTC (GÖRÜNTÜLÜ/SESLİ GÖRÜŞME) SİNYALLEŞME METOTLARI
        // ========================================================

        // 1. Arayan kişi, karşı tarafa "Seni arıyorum, bunlar kamera ayarlarım (Offer)" der
        public async Task CallUser(Guid receiverId, string offer)
        {
            var callerIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_connectedUsers.TryGetValue(receiverId, out string? connectionId))
            {
                // Karşı tarafın ekranına "Seni arıyorlar" sinyali düşür
                await Clients.Client(connectionId).SendAsync("IncomingCall", callerIdStr, offer);
            }
        }

        // 2. Aranan kişi açmayı kabul ederse "Kabul ettim, bunlar da benim ayarlarım (Answer)" der
        public async Task AnswerCall(Guid callerId, string answer)
        {
            var receiverIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_connectedUsers.TryGetValue(callerId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CallAnswered", receiverIdStr, answer);
            }
        }

        // 3. Aranan kişi aramayı reddederse
        public async Task RejectCall(Guid callerId)
        {
            var receiverIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_connectedUsers.TryGetValue(callerId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CallRejected", receiverIdStr);
            }
        }

        // 4. Görüşme sırasında biri telefonu kapatırsa
        public async Task EndCall(Guid targetId)
        {
            var senderIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_connectedUsers.TryGetValue(targetId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CallEnded", senderIdStr);
            }
        }

        // 5. İki tarafın birbirinin ağ/IP yolunu bulması için gereken güvenlik paketleri (ICE Candidates)
        public async Task SendIceCandidate(Guid targetId, string candidate)
        {
            var senderIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_connectedUsers.TryGetValue(targetId, out string? connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveIceCandidate", senderIdStr, candidate);
            }
        }
        // ========================================================
        // MAVİ TİK (OKUNDU BİLGİSİ) SİNYALİ
        // ========================================================
        public async Task MarkMessagesAsRead(Guid senderId)
        {
            var receiverIdStr = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(receiverIdStr, out Guid receiverId)) return;

            // 1. Veritabanındaki okunmamış mesajları bul ve true yap
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();

                // 2. Mesajı gönderen kişiye (Eğer o an aktifse) "Senin mesajlar okundu, Mavi Tik yap!" de
                if (_connectedUsers.TryGetValue(senderId, out string? connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessagesRead", receiverId.ToString());
                }
            }
        }
    }
}