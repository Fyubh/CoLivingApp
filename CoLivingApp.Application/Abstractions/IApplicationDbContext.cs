// Файл: CoLivingApp.Application/Abstractions/IApplicationDbContext.cs
using CoLivingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Apartment> Apartments { get; }
    DbSet<ApartmentMember> ApartmentMembers { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<ExpenseSplit> ExpenseSplits { get; }
    DbSet<Settlement> Settlements { get; }
    DbSet<ProductCatalog> ProductCatalogs { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Chore> Chores { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}