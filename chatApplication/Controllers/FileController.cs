using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace chatApplication.Controllers;

[Route("api/File")] // DİKKAT: Burası api/File oldu
[ApiController]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IWebHostEnvironment _environment;

    public ImageController(IImageService imageService, IWebHostEnvironment environment)
    {
        _imageService = imageService;
        _environment = environment;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadEncryptedImage(IFormFile file)
    {
        try
        {
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fileUrl = await _imageService.UploadEncryptedImageAsync(file, webRootPath);

            // DİKKAT: EncryptedImageUrl yerine sadece "url" yazıyoruz
            return Ok(new { url = fileUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}