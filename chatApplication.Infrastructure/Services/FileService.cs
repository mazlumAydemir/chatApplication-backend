using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace chatApplication.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public FileService(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        public async Task<string> UploadEncryptedImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Dosya boş.");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ".enc";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{uniqueFileName}";
        }

        public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Dosya boş.");

            var avatarsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
            if (!Directory.Exists(avatarsFolder)) Directory.CreateDirectory(avatarsFolder);

            var uniqueFileName = userId.ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(avatarsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Veritabanını güncelle
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.ProfilePictureUrl = $"/avatars/{uniqueFileName}";
                await _context.SaveChangesAsync();
            }

            return $"/avatars/{uniqueFileName}";
        }
    }
}