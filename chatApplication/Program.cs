using chatApplication.Application;
using chatApplication.Infrastructure;
using chatApplication.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using chatApplication.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. TEMEL SERVİSLER VE CORS AYARLARI
// ==========================================
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://158.220.105.185:5173",
                "http://chat.mazlumaydemir.online",
                "https://chat.mazlumaydemir.online"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ==========================================
// 2. SIGNALR VE KİMLİK EŞLEŞTİRME
// ==========================================
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// ==========================================
// 3. REDIS VE VERİTABANI BAĞLANTILARI
// ==========================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

// ==========================================
// 4. KATMANLARIN (DEPENDENCY INJECTION) KAYDI
// ==========================================
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ==========================================
// 5. JWT KİMLİK DOĞRULAMA (AUTHENTICATION)
// ==========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ==========================================
// 6. SWAGGER / OPENAPI AYARLARI
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IEA Chat API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();
app.UsePathBase("/chat-backend");
// ==========================================
// ⚠️ KRİTİK: UPLOADS KLASÖRÜNÜ HEMEN OLUŞTUR
// UseStaticFiles'den ÖNCE olmalı!
// ==========================================

// ✅ DÜZELTME: Proje root'undaki uploads klasörünü bul
var uploadsPath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,  // /bin/Debug/net8.0 veya /bin/Release/net8.0
    "..", "..", "..", "uploads"             // Proje root'una çık, uploads'a git
);
uploadsPath = Path.GetFullPath(uploadsPath);  // Gerçek path'i al

try
{
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine($"✅ Uploads klasörü oluşturuldu: {uploadsPath}");
    }
    else
    {
        Console.WriteLine($"✅ Uploads klasörü zaten var: {uploadsPath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Uploads klasörü oluşturulamadı: {ex.Message}");
    throw;
}

// ==========================================
// 7. HTTP REQUEST PIPELINE (MIDDLEWARE'LER)
// ==========================================

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// ✅ DÜZELTME: Swagger'ın Nginx /chat-backend/ alt yolundan düzgün çalışması için güncellendi
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/chat-backend/swagger/v1/swagger.json", "IEA Chat API v1");
    c.RoutePrefix = "swagger";
});

// ✅ KRİTİK DEĞİŞİKLİK: CORS, UseStaticFiles'dan ÖNCE çağrılmalı!
app.UseCors("AllowReactApp");

// .enc Uzantılı Şifreli Dosyalara İzin Verme Ayarı
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".enc"] = "text/plain";

// 📁 wwwroot klasörünü sun
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// 📁 uploads klasörünü sun
if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads",
        ContentTypeProvider = provider
        // OnPrepareResponse blokunu kaldırdık çünkü UseCors artık bunu global olarak hallediyor.
    });
    Console.WriteLine("✅ /uploads endpoint'i aktif");
}
else
{
    Console.WriteLine($"❌ /uploads endpoint'i etkinleştirilemedi - klasör yok: {uploadsPath}");
}

app.UseHttpsRedirection();  // ✅ HTTPS redirect
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// ==========================================
// DATABASE MIGRATION
// ==========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Console.WriteLine("✅ Database migration başarılı");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration hatası: {ex.Message}");
    }
}

Console.WriteLine("🚀 Uygulama başlatıldı!");
Console.WriteLine($"📁 Uploads Klasörü: {uploadsPath}");
app.Run();

// ==========================================
// YARDIMCI SINIFLAR
// ==========================================

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}