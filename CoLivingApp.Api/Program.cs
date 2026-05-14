using CoLivingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CoLivingApp.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CoLivingApp.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        var configuredOrigins = builder.Configuration
            .GetSection("AllowedCorsOrigins")
            .Get<string[]>();

        var origins = configuredOrigins is { Length: > 0 }
            ? configuredOrigins
            : new[]
            {
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            };

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ (Dependency Injection)
// ==========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddSignalR();
builder.Services.AddHostedService<SchedulerBackgroundService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CoLivingApp.Application.Features.Apartments.Commands.CreateApartment.CreateApartmentCommand).Assembly));

// НАСТРОЙКА JWT АВТОРИЗАЦИИ
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("JwtSettings:Secret must be configured and at least 32 characters long.");

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
            // RoleClaimType намеренно НЕ переопределяем. JwtBearer по умолчанию
            // включает inbound claim mapping: короткий "role" из JWT превращается
            // в ClaimTypes.Role в ClaimsPrincipal. Дефолтный RoleClaimType ==
            // ClaimTypes.Role — то есть всё совпадает. Если зафорсить
            // RoleClaimType="role", [Authorize(Roles=...)] начинает искать
            // claim с типом "role", а в principal он лежит под URI — итог 403.
        };

        // SignalR через WebSocket не может проставить заголовок Authorization
        // (браузерные WS-handshake'и его не пускают), поэтому стандарт —
        // передавать токен в query-параметре `access_token` только для путей
        // хабов. На обычные /api/* эндпоинты это не влияет.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(); // Обязательно добавляем сервисы авторизации
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 3. CORS (Разрешаем кросс-доменные запросы)
app.UseCors("AppCors");

// 4. Аутентификация и Авторизация (СТРОГО в таком порядке)
app.UseAuthentication();
app.Use(async (context, next) =>
{
    var user = context.User;
    var mustChangePassword = user.Identity?.IsAuthenticated == true
                             && user.HasClaim("mustChangePassword", "true");

    if (mustChangePassword
        && context.Request.Path.StartsWithSegments("/api")
        && !context.Request.Path.StartsWithSegments("/api/Users/change-password"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Перед продолжением необходимо сменить временный пароль."
        });
        return;
    }

    await next();
});
app.UseAuthorization();

// 5. Маппинг контроллеров и сокетов
app.MapControllers();
app.MapHub<CoLivingApp.Api.Hubs.CoLivingHub>("/hubs/coliving")
    .RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await CoLivingApp.Infrastructure.Seeders.TheFizzPragueSeeder.SeedAsync(app.Services);
}

app.Run();
