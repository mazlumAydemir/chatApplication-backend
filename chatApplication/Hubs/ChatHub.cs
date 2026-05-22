using chatApplication.Application.Interfaces.Services;
using chatApplication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;

namespace chatApplication.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IMessageService messageService, IUserService userService, ILogger<ChatHub> logger)
    {
        _messageService = messageService;
        _userService = userService;
        _logger = logger;
    }

    public async Task SendEncryptedPayload(int receiverId, string payload)
    {
        _logger.LogInformation("════════════════════════════════════════════════════════════");
        _logger.LogInformation("🔄 SEND ENCRYPTED PAYLOAD - START");
        _logger.LogInformation("════════════════════════════════════════════════════════════");
        try
        {
            // STEP 1: SENDER ID KONTROL
            _logger.LogInformation("STEP 1: Sender ID kontrol ediliyor...");
            var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"  senderIdStr: '{senderIdStr}'");
            if (string.IsNullOrEmpty(senderIdStr) || !int.TryParse(senderIdStr, out int senderId))
            {
                _logger.LogError("  ❌ HATA: Geçersiz Sender ID!");
                await Clients.Caller.SendAsync("Error", "Kullanıcı kimliği doğrulanamadı");
                return;
            }
            _logger.LogInformation($"  ✅ Sender ID: {senderId}");

            // STEP 2: RECEIVER ID KONTROL
            _logger.LogInformation($"STEP 2: Receiver ID: {receiverId}");

            // STEP 3: PAYLOAD KONTROL
            _logger.LogInformation($"STEP 3: Payload kontrol ediliyor...");
            _logger.LogInformation($"  Payload length: {payload?.Length ?? 0}");
            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogError("  ❌ HATA: Payload boş!");
                await Clients.Caller.SendAsync("Error", "Mesaj boş olamaz");
                return;
            }
            _logger.LogInformation($"  ✅ Payload OK (ilk 150 char): {payload.Substring(0, Math.Min(150, payload.Length))}");

            // STEP 4: JSON PARSE
            _logger.LogInformation("STEP 4: JSON Parse ediliyor...");
            JsonDocument payloadObj = null;
            try
            {
                payloadObj = JsonDocument.Parse(payload);
                _logger.LogInformation("  ✅ JSON Parse başarılı");
            }
            catch (JsonException ex)
            {
                _logger.LogError($"  ❌ JSON Parse hatası: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "JSON formatı geçersiz");
                return;
            }

            // STEP 5: PAYLOAD FIELDS EXTRACTION
            _logger.LogInformation("STEP 5: Payload fields çıkarılıyor...");
            var root = payloadObj.RootElement;
            string encryptedData = null;
            string encryptedSessionKey = null;
            string senderEncryptedSessionKey = null;
            string digitalSignature = null;

            // encryptedData
            _logger.LogInformation("  → encryptedData çıkarılıyor...");
            if (root.TryGetProperty("encryptedData", out var encDataProp))
            {
                _logger.LogInformation($"    JsonValueKind: {encDataProp.ValueKind}");
                try
                {
                    encryptedData = ConvertToString(encDataProp);
                    if (encryptedData != null)
                        _logger.LogInformation($"    ✅ encryptedData: {encryptedData.Length} chars");
                    else
                        _logger.LogWarning($"    ⚠️ encryptedData null!");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"    ❌ Conversion error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("  ⚠️ encryptedData property bulunamadı!");
            }

            // encryptedSessionKey
            _logger.LogInformation("  → encryptedSessionKey çıkarılıyor...");
            if (root.TryGetProperty("encryptedSessionKey", out var encSessionProp))
            {
                _logger.LogInformation($"    JsonValueKind: {encSessionProp.ValueKind}");
                try
                {
                    encryptedSessionKey = ConvertToString(encSessionProp);
                    if (encryptedSessionKey != null)
                        _logger.LogInformation($"    ✅ encryptedSessionKey: {encryptedSessionKey.Length} chars");
                    else
                        _logger.LogWarning($"    ⚠️ encryptedSessionKey null!");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"    ⚠️ Conversion warning: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("  ⚠️ encryptedSessionKey property bulunamadı!");
            }

            // senderEncryptedSessionKey
            _logger.LogInformation("  → senderEncryptedSessionKey çıkarılıyor...");
            if (root.TryGetProperty("senderEncryptedSessionKey", out var senderEncSessionProp))
            {
                _logger.LogInformation($"    JsonValueKind: {senderEncSessionProp.ValueKind}");
                try
                {
                    senderEncryptedSessionKey = ConvertToString(senderEncSessionProp);
                    if (senderEncryptedSessionKey != null)
                        _logger.LogInformation($"    ✅ senderEncryptedSessionKey: {senderEncryptedSessionKey.Length} chars");
                    else
                        _logger.LogWarning($"    ⚠️ senderEncryptedSessionKey null!");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"    ⚠️ Conversion warning: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("  ⚠️ senderEncryptedSessionKey property bulunamadı!");
            }

            // signature
            _logger.LogInformation("  → signature çıkarılıyor...");
            if (root.TryGetProperty("signature", out var sigProp))
            {
                _logger.LogInformation($"    JsonValueKind: {sigProp.ValueKind}");
                try
                {
                    digitalSignature = ConvertToString(sigProp);
                    if (digitalSignature != null)
                        _logger.LogInformation($"    ✅ signature: {digitalSignature.Length} chars");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"    ⚠️ Conversion warning: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("  ⚠️ signature property bulunamadı (opsiyonel)");
            }

            // ✅ STEP 5B: BASE64 VALIDATION
            _logger.LogInformation("STEP 5B: Base64 Validation...");

            // encryptedSessionKey validation
            if (string.IsNullOrEmpty(encryptedSessionKey))
            {
                _logger.LogError("  ❌ HATA: encryptedSessionKey NULL!");
                await Clients.Caller.SendAsync("Error", "Şifreli session key bulunamadı");
                return;
            }

            try
            {
                Convert.FromBase64String(encryptedSessionKey);
                _logger.LogInformation($"  ✅ encryptedSessionKey valid Base64");
            }
            catch (Exception ex)
            {
                _logger.LogError($"  ❌ HATA: encryptedSessionKey INVALID BASE64!");
                _logger.LogError($"     Value: {encryptedSessionKey.Substring(0, Math.Min(100, encryptedSessionKey.Length))}...");
                _logger.LogError($"     Exception: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "Şifreli session key format geçersiz (Base64)");
                return;
            }

            // senderEncryptedSessionKey validation
            if (string.IsNullOrEmpty(senderEncryptedSessionKey))
            {
                _logger.LogError("  ❌ HATA: senderEncryptedSessionKey NULL!");
                await Clients.Caller.SendAsync("Error", "Gönderici şifreli session key bulunamadı");
                return;
            }

            try
            {
                Convert.FromBase64String(senderEncryptedSessionKey);
                _logger.LogInformation($"  ✅ senderEncryptedSessionKey valid Base64");
            }
            catch (Exception ex)
            {
                _logger.LogError($"  ❌ HATA: senderEncryptedSessionKey INVALID BASE64!");
                _logger.LogError($"     Value: {senderEncryptedSessionKey.Substring(0, Math.Min(100, senderEncryptedSessionKey.Length))}...");
                _logger.LogError($"     Exception: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "Gönderici şifreli session key format geçersiz (Base64)");
                return;
            }

            // STEP 6: VALIDATION
            _logger.LogInformation("STEP 6: Validation...");
            if (string.IsNullOrEmpty(encryptedData))
            {
                _logger.LogError("  ❌ HATA: encryptedData boş!");
                await Clients.Caller.SendAsync("Error", "Şifreli veri bulunamadı");
                return;
            }
            _logger.LogInformation("  ✅ Validation passed");

            // STEP 7: MESSAGE OBJECT OLUŞTUR
            // ✅ MİMARİ DÜZELTME: Tüm veriler TextContent'e gider, EncryptedImageUrl kullanılmıyor
            _logger.LogInformation("STEP 7: Message object oluşturuluyor...");
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                TextContent = encryptedData,  // ✅ Tüm veriler buraya (text/gallery/image)
                EncryptedImageUrl = null,     // ✅ Artık kullanılmıyor
                EncryptedSessionKey = encryptedSessionKey,
                SenderEncryptedSessionKey = senderEncryptedSessionKey,
                DigitalSignature = digitalSignature,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };
            _logger.LogInformation($"  ✅ Message object created:");
            _logger.LogInformation($"    - SenderId: {message.SenderId}");
            _logger.LogInformation($"    - ReceiverId: {message.ReceiverId}");
            _logger.LogInformation($"    - TextContent: {(message.TextContent != null ? "yes" : "null")}");
            _logger.LogInformation($"    - EncryptedImageUrl: {(message.EncryptedImageUrl != null ? "yes" : "null")}");
            _logger.LogInformation($"    - EncryptedSessionKey: {(message.EncryptedSessionKey != null ? "yes (" + message.EncryptedSessionKey.Length + " chars)" : "NULL")}");
            _logger.LogInformation($"    - SenderEncryptedSessionKey: {(message.SenderEncryptedSessionKey != null ? "yes (" + message.SenderEncryptedSessionKey.Length + " chars)" : "NULL")}");

            // STEP 8: DATABASE SAVE
            _logger.LogInformation("STEP 8: Database'e kaydediliyor...");
            try
            {
                await _messageService.AddMessageAsync(message);
                _logger.LogInformation("  ✅ AddMessageAsync başarılı - MESSAGE SAVED!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"  ❌ AddMessageAsync ERROR: {ex.Message}");
                _logger.LogError($"     Stack: {ex.StackTrace}");
                await Clients.Caller.SendAsync("Error", $"Database error: {ex.Message}");
                return;
            }

            // STEP 9: SEND TO CLIENT
            _logger.LogInformation("STEP 9: Frontend'e gönderiliyor...");
            try
            {
                await Clients.User(receiverId.ToString()).SendAsync("ReceivePayload", payload);
                _logger.LogInformation($"  ✅ ReceivePayload gönderildi user {receiverId}'ye");
            }
            catch (Exception ex)
            {
                _logger.LogError($"  ⚠️ SendAsync warning: {ex.Message}");
            }

            _logger.LogInformation("════════════════════════════════════════════════════════════");
            _logger.LogInformation("✅ SEND ENCRYPTED PAYLOAD - SUCCESS");
            _logger.LogInformation("════════════════════════════════════════════════════════════");
        }
        catch (Exception ex)
        {
            _logger.LogError("════════════════════════════════════════════════════════════");
            _logger.LogError($"❌ SEND ENCRYPTED PAYLOAD - GENERAL ERROR");
            _logger.LogError($"Exception: {ex.Message}");
            _logger.LogError($"StackTrace: {ex.StackTrace}");
            _logger.LogError("════════════════════════════════════════════════════════════");
            await Clients.Caller.SendAsync("Error", "Mesaj gönderilemedi");
        }
    }

    private string ConvertToString(JsonElement element)
    {
        try
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetInt32().ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Array => element.GetRawText(),
                _ => element.GetRawText()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"ConvertToString error ({element.ValueKind}): {ex.Message}");
            return null;
        }
    }

    public async Task SendTyping(int targetUserId, bool isTyping)
    {
        try
        {
            var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdStr)) return;
            await Clients.User(targetUserId.ToString()).SendAsync("ReceiveTyping", senderIdStr, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError($"SendTyping Error: {ex.Message}");
        }
    }

    public async Task MarkMessagesAsRead(int senderId)
    {
        try
        {
            var readerIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(readerIdStr) || !int.TryParse(readerIdStr, out int readerId)) return;
            await _messageService.MarkMessagesAsReadAsync(senderId, readerId);
            await Clients.User(senderId.ToString()).SendAsync("MessagesRead", readerId);
        }
        catch (Exception ex)
        {
            _logger.LogError($"MarkRead Error: {ex.Message}");
        }
    }

    public async Task CallUser(int targetUserId, string payload)
    {
        try
        {
            var callerIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(callerIdStr)) return;
            await Clients.User(targetUserId.ToString()).SendAsync("IncomingCall", callerIdStr, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError($"CallUser Error: {ex.Message}");
        }
    }

    public async Task AnswerCall(int callerId, string payload)
    {
        try
        {
            var receiverIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(receiverIdStr)) return;
            await Clients.User(callerId.ToString()).SendAsync("CallAnswered", receiverIdStr, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError($"AnswerCall Error: {ex.Message}");
        }
    }

    public async Task RejectCall(int callerId)
    {
        try
        {
            await Clients.User(callerId.ToString()).SendAsync("CallRejected");
        }
        catch (Exception ex)
        {
            _logger.LogError($"RejectCall Error: {ex.Message}");
        }
    }

    public async Task EndCall(int targetUserId)
    {
        try
        {
            await Clients.User(targetUserId.ToString()).SendAsync("CallEnded");
        }
        catch (Exception ex)
        {
            _logger.LogError($"EndCall Error: {ex.Message}");
        }
    }

    public async Task SendIceCandidate(int targetUserId, string candidate)
    {
        try
        {
            var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdStr)) return;
            await Clients.User(targetUserId.ToString()).SendAsync("ReceiveIceCandidate", senderIdStr, candidate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"SendIce Error: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return;
            await _userService.UpdateUserStatusAsync(userId, true);
            _logger.LogInformation($"✅ User connected: {userId}");
            await Clients.Others.SendAsync("UserStatusChanged", userIdStr, true, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError($"OnConnected Error: {ex.Message}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return;
            await _userService.UpdateUserStatusAsync(userId, false);
            _logger.LogInformation($"❌ User disconnected: {userId}");
            await Clients.Others.SendAsync("UserStatusChanged", userIdStr, false, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError($"OnDisconnected Error: {ex.Message}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}