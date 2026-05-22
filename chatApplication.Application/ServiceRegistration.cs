using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chatApplication.Application.Interfaces.Services;
using chatApplication.Application.Services;
using chatApplication.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace chatApplication.Application;

public static class ServiceRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // AuthService'i sisteme tanıtıyoruz
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IImageService, ImageService>(); 
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
    }
}