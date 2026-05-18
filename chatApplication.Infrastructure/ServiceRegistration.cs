using chatApplication.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using chatApplication.Application.Interfaces.Repositories;
using chatApplication.Application.Interfaces.Services;
using chatApplication.Infrastructure.Persistence.Contexts;
using chatApplication.Infrastructure.Repositories;
using chatApplication.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace chatApplication.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext Kaydı
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // TokenService Kaydı
        services.AddScoped<ITokenService, TokenService>();

        // Generic Repository Kaydı (Hatanın sebebi olan eksik satır)
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    }
}