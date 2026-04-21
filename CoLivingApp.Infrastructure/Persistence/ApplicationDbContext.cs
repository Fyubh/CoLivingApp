// Файл: ApplicationDbContext.cs
using System.Reflection;
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Common;
using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CoLivingApp.Infrastructure.Persistence;

/// <summary>
/// Главный класс для работы с базой данных PostgreSQL.
/// Служит "мостом" между нашими C# классами и реальными таблицами в БД.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // === Старые DbSet (roommate-layer) ===
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

    // === НОВЫЕ DbSet: building-layer ===
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();

    // === Staff & Maintenance layer ===
    public DbSet<StaffAssignment> StaffAssignments => Set<StaffAssignment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматически находит и применяет все IEntityTypeConfiguration из сборки.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Переопределяем SaveChanges, чтобы автоматически заполнять UpdatedAt для EntityBase.
    /// Так нигде в handler'ах не нужно руками писать entity.UpdatedAt = DateTime.UtcNow.
    /// 
    /// ПРИМЕЧАНИЕ: CreatedById/UpdatedById пока НЕ заполняем автоматически — это потребует
    /// ICurrentUserService, который добавим, когда появится первый handler работающий
    /// с новыми entities. Пока заполнение ложится на Command handler.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<EntityBase>();
        var now = DateTime.UtcNow;

        foreach (EntityEntry<EntityBase> entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // CreatedAt уже выставлен дефолтом в самой entity,
                    // но на случай если кто-то перезаписал — оставляем как есть.
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // Защита от случайного изменения CreatedAt через tracked update
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Конвертируем hard delete в soft delete.
                    // Если реально нужен физический delete — делается через raw SQL или отдельный метод.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}