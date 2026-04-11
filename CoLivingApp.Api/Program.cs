using CoLivingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CoLivingApp.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ (Dependency Injection)
// ==========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CoLivingApp.Application.Features.Apartments.Commands.CreateApartment.CreateApartmentCommand).Assembly));
// НАСТРОЙКА JWT АВТОРИЗАЦИИ
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(); // Обязательно добавляем сервисы авторизации
builder.Services.AddControllers();

// Настройка Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Собираем приложение
var app = builder.Build();

// ==========================================
// 2. НАСТРОЙКА MIDDLEWARE (Пайплайн запросов)
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();

app.UseStaticFiles();
    
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<CoLivingApp.Api.Hubs.CoLivingHub>("/hubs/coliving");

app.Run();