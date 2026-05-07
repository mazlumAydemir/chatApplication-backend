using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapmış kullanıcılar resim yükleyebilir veya indirebilir
    public class ImageController : ControllerBase
    {
        // Şifreli resimlerin kaydedileceği klasörün yolu
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "EncryptedImages");

        public ImageController()
        {
            // Eğer sunucuda bu klasör yoksa, otomatik olarak oluştur
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Geçersiz veya boş dosya.");

            // Güvenlik için dosyanın orijinal adını çöpe atıp, ona eşsiz bir Guid adı veriyoruz.
            // Uzantısını da ".enc" (encrypted) yaparak şifreli olduğunu belli ediyoruz.
            var fileName = Guid.NewGuid().ToString() + ".enc";
            var filePath = Path.Combine(_uploadFolder, fileName);

            // Gelen dosyayı sunucudaki klasöre kopyalıyoruz
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Resmi gönderen kişiye, resmin sunucudaki yeni adını geri döndürüyoruz
            // (Bu sayede bu adı SignalR ile karşı tarafa mesaj olarak fırlatabilecek)
            return Ok(new { FileName = fileName, Message = "Şifreli dosya başarıyla yüklendi." });
        }

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadImage(string fileName)
        {
            var filePath = Path.Combine(_uploadFolder, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("İstenen dosya sunucuda bulunamadı.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Dosyayı resim (jpeg/png) olarak DEĞİL, "application/octet-stream" yani ham veri (byte) olarak döndürüyoruz.
            // Çünkü bu şifreli bir dosya, şifresi ancak istemci tarafında çözülecek.
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}