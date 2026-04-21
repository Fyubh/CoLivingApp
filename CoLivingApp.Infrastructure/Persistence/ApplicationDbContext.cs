// Файл: ApplicationDbContext.cs
using System.Reflection;
using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoLivingApp.Application.Abstractions;

namespace CoLivingApp.Infrastructure.Persistence;

/// <summary>
/// Главный класс для работы с базой данных PostgreSQL.
/// Служит "мостом" между нашими C# классами и реальными таблицами в БД.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    // Конструктор принимает настройки (например, строку подключения к БД)
    // и передает их в базовый класс DbContext.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) 
    { 
    }

    // DbSet - это представление таблицы в базе данных. 
    // Через эти свойства мы будем делать запросы (LINQ) и сохранять данные.
    public DbSet<User> Users => Set<User>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<ApartmentMember> ApartmentMembers => Set<ApartmentMember>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<ProductCatalog> ProductCatalogs => Set<ProductCatalog>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Chore> Chores => Set<Chore>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();
    public DbSet<RecurringChore> RecurringChores => Set<RecurringChore>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Incident> Incidents => Set<Incident>();


    /// <summary>
    /// Этот метод вызывается один раз при старте приложения, когда EF Core 
    /// строит модель базы данных в памяти.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Магия чистого кода: вместо того чтобы писать сотни строк настроек прямо здесь,
        // мы говорим EF Core: "Просканируй этот проект и найди все классы, 
        // которые реализуют IEntityTypeConfiguration, и примени их".
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}