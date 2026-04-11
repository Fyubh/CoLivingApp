using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Inventory.Commands.CreateItem;

public record CreateItemCommand(
    Guid ApartmentId, string? CustomName, decimal Quantity, UnitType Unit, ItemStatus Status, string UserId,
    ItemCategory Category, StorageLocation Location, DateTime? ExpiryDate
) : IRequest<Result<Guid>>;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // МАГИЯ ЗДЕСЬ: Если дата есть, принудительно переводим её в формат UTC для PostgreSQL
        DateTime? safeUtcExpiryDate = request.ExpiryDate.HasValue 
            ? DateTime.SpecifyKind(request.ExpiryDate.Value, DateTimeKind.Utc) 
            : null;

        var item = new InventoryItem
        {
            ApartmentId = request.ApartmentId,
            CustomName = request.CustomName,
            Quantity = request.Quantity,
            Unit = request.Unit,
            Status = request.Status,
            Category = request.Category,
            Location = request.Location,
            ExpiryDate = safeUtcExpiryDate, // Сохраняем безопасную дату
            CreatedById = request.UserId,
            UpdatedById = request.UserId,
            PurchasedAt = request.Status == ItemStatus.Available ? DateTime.UtcNow : null
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(item.Id);
    }
}