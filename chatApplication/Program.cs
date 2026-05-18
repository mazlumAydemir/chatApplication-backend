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
var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. TEMEL SERVÝSLER VE CORS
// ==========================================
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://158.220.105.185:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR için zorunlu
    });
});

// ==========================================
// 2. SIGNALR VE KÝMLÝK EÞLEÞTÝRME (KRÝTÝK KOD)
// ==========================================
builder.Services.AddSignalR();

// SignalR'ýn baðlanan kullanýcýlarý Token'daki NameIdentifier (Kullanýcý ID) ile eþleþtirmesini saðlar
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// ==========================================
// 3. REDIS VE VERÝTABANI BAÐLANTILARI
// ==========================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

// DbContext, önceki adýmlarda AddInfrastructureServices içine eklendiði için
// burada tekrar yazmaya gerek yok, karmaþýklýðý önler.

// ==========================================
// 4. KATMANLARIN (DEPENDENCY INJECTION) KAYDI
// ==========================================
// Yazdýðýmýz Extension metodlarý çaðýrarak servislerimizi (.NET'e) tanýtýyoruz
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ==========================================
// 5. JWT KÝMLÝK DOÐRULAMA (AUTHENTICATION)
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

        // SignalR için WebSockets üzerinden gelen Token'ý okuma ayarý
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
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
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

// ==========================================
// 7. HTTP REQUEST PIPELINE (MIDDLEWARE'LER)
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// .enc Uzantýlý Þifreli Dosyalara Ýzin Verme Ayarý
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".enc"] = "text/plain";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseCors("AllowReactApp");

// DÝKKAT: Authentication her zaman Authorization'dan önce gelmelidir!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub'ýný dýþarý açýyoruz (ChatHub sýnýfýný oluþturduðunda bu endpoint aktif olacak)
 app.MapHub<ChatHub>("/chathub"); 

// Veritabaný Migration'larýný otomatik uygulama
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

// ==========================================
// YARDIMCI SINIFLAR
// ==========================================

/// <summary>
/// SignalR'ýn, JWT içerisindeki ID'yi (NameIdentifier) Connection ile eþleþtirmesi için gerekli sýnýf.
/// </summary>
public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // TokenService içerisinde belirlediðimiz ClaimTypes.NameIdentifier (User.Id) deðerini çeker
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}