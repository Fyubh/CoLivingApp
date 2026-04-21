// Файл: CoLivingApp.Application/Abstractions/IApplicationDbContext.cs
using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Abstractions;

public interface IApplicationDbContext
{
    // === Roommate layer ===
    DbSet<User> Users { get; }
    DbSet<Apartment> Apartments { get; }
    DbSet<ApartmentMember> ApartmentMembers { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<ExpenseSplit> ExpenseSplits { get; }
    DbSet<Settlement> Settlements { get; }
    DbSet<ProductCatalog> ProductCatalogs { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Chore> Chores { get; }
    DbSet<RecurringExpense> RecurringExpenses { get; }
    DbSet<RecurringChore> RecurringChores { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<Incident> Incidents { get; }

    // === Building layer ===
    DbSet<Operator> Operators { get; }
    DbSet<Building> Buildings { get; }
    DbSet<Floor> Floors { get; }
    DbSet<Room> Rooms { get; }

    // === Staff & Maintenance layer (новое) ===
    DbSet<StaffAssignment> StaffAssignments { get; }
    DbSet<MaintenanceRequest> MaintenanceRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}