using chatApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        // DÜZELTME: [FromForm] etiketini SİLDİK! (Swagger'ın çökmemesi için)
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Dosya sunucuya ulaşamadı.");
                }

                var fileUrl = await _fileService.UploadEncryptedImageAsync(file);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("upload-avatar")]
        // DÜZELTME: [FromForm] etiketini SİLDİK!
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Avatar dosyası sunucuya ulaşamadı.");
                }

                var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var fileUrl = await _fileService.UploadAvatarAsync(file, currentUserId);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}