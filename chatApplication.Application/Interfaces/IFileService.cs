using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace chatApplication.Application.Interfaces
{
    public interface IFileService
    {
        // Şifreli sohbet resimlerini yüklemek için
        Task<string> UploadEncryptedImageAsync(IFormFile file);

        // Kullanıcı profil fotoğrafını yükleyip veritabanını güncellemek için
        Task<string> UploadAvatarAsync(IFormFile file, Guid userId);
    }
}