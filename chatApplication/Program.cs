using chatApplication.Application.Interfaces;
using chatApplication.Application.Services;
using chatApplication.Infrastructure.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// CORS Ayarlarý (React uygulamasýnýn API'ye eriþebilmesi için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://158.220.105.185:5173") // React projesinin çalýþtýðý port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Ýleride SignalR baðlantýsý için bu çok önemli!
    });
});

builder.Services.AddSignalR(); // SignalR servisini projeye dahil eder

// Redis Baðlantýsý
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

// JWT Kimlik Doðrulama Ayarlarý
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // SignalR için Token okuma ayarý
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Ýstek hub'a geliyorsa ve token URL'de (Query) varsa, oradan al
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Entity Framework Core'u MSSQL ile kullanmak için ekliyoruz
builder.Services.AddDbContext<chatApplication.Infrastructure.Contexts.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// SERVÝS KATMANI KAYITLARI (DEPENDENCY INJECTION)
// ==========================================
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// EKSÝK OLAN VE 500 HATASINI ÇÖZEN SATIR:
builder.Services.AddScoped<IFileService, FileService>();

// Email Servisi
builder.Services.AddScoped<chatApplication.Application.Interfaces.IEmailService, chatApplication.Infrastructure.Services.EmailService>();
// ==========================================

// Swagger/OpenAPI Ayarlarý (JWT Butonu Eklendi!)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IEA Chat API", Version = "v1" });

    // Swagger ekranýna "Authorize" butonunu koyar
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// ÇOK ÖNEMLÝ: Kimlik doðrulama, Yetkilendirmeden (Authorization) ÖNCE gelmek zorundadýr!
// --- .enc Uzantýlý Þifreli Dosyalara Ýzin Verme Ayarý ---
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".enc"] = "text/plain"; // .enc uzantýsýnýn bir metin olduðunu sunucuya öðretiyoruz

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
// --------------------------------------------------------

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<chatApplication.Hubs.ChatHub>("/chathub"); // Santralin adresini belirliyoruz
app.MapControllers();

// Veritabaný Migration'larýný otomatik uygula
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();