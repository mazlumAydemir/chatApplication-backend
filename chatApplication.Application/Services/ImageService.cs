using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace chatApplication.Application.Services;

public class ImageService : IImageService
{
    public async Task<string> UploadEncryptedImageAsync(IFormFile file, string webRootPath)
    {
        if (file == null || file.Length == 0)
            throw new Exception("Geçersiz dosya.");

        var extension = Path.GetExtension(file.FileName);
        if (extension.ToLower() != ".enc")
            throw new Exception("Sadece DES ile şifrelenmiş (.enc) dosyalar yüklenebilir.");

        var uploadsFolder = Path.Combine(webRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + ".enc";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{uniqueFileName}";

    }
}