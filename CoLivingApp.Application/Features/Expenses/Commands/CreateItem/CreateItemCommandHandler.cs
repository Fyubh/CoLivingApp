// Файл: CoLivingApp.Application/Features/Inventory/Commands/CreateItem/CreateItemCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Inventory.Commands.CreateItem;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem
        {
            ApartmentId = request.ApartmentId,
            CustomName = request.CustomName,
            Quantity = request.Quantity,
            Unit = request.Unit,
            Status = request.Status,
            CreatedById = request.UserId,
            UpdatedById = request.UserId,
            PurchasedAt = request.Status == ItemStatus.Available ? DateTime.UtcNow : null
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(item.Id);
    }
}