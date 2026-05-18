using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace chatApplication.Application.Interfaces.Services;

public interface IImageService
{
    Task<string> UploadEncryptedImageAsync(IFormFile file, string webRootPath);
}