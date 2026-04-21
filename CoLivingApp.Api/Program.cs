using CoLivingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CoLivingApp.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CoLivingApp.Infrastructure.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ДОБАВЛЯЕМ СЕРВИС CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Разрешает запросы с любых портов и доменов
            .AllowAnyMethod()  // Разрешает GET, POST, PUT, DELETE
            .AllowAnyHeader(); // Разрешает любые заголовки (очень важно для передачи токена Authorization!)
    });
});

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ (Dependency Injection)
// ==========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSignalR();
builder.Services.AddHostedService<SchedulerBackgroundService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CoLivingApp.Application.Features.Apartments.Commands.CreateApartment.CreateApartmentCommand).Assembly));

// ДОБАВЛЯЕМ CORS (Разрешаем React-приложению делать запросы)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel", policy =>
    {
        // Укажи здесь порт твоего Vite-приложения (обычно 5173)
        policy.WithOrigins("http://localhost:5173") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Важно для SignalR в будущем
    });
});

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

// 1. Отдаем статические файлы (твой index.html) в самом начале!
app.UseDefaultFiles();
app.UseStaticFiles();

// 2. Swagger для тестирования API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAdminPanel");

// ВНИМАНИЕ: app.UseHttpsRedirection() УДАЛЕН, чтобы не ломать локальные запросы по HTTP!

// 3. CORS (Разрешаем кросс-доменные запросы)
app.UseCors("AllowAll");

// 4. Аутентификация и Авторизация (СТРОГО в таком порядке)
app.UseAuthentication();
app.UseAuthorization();

// 5. Маппинг контроллеров и сокетов
app.MapControllers();
app.MapHub<CoLivingApp.Api.Hubs.CoLivingHub>("/hubs/coliving");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await CoLivingApp.Infrastructure.Seeders.TheFizzPragueSeeder.SeedAsync(app.Services);
}

app.Run();