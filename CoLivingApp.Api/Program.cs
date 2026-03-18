using CoLivingApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CoLivingApp.Application.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. РЕГИСТРАЦИЯ СЕРВИСОВ (Dependency Injection)
// ==========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CoLivingApp.Application.Features.Apartments.Commands.CreateApartment.CreateApartmentCommand).Assembly));

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



app.Run();