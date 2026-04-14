using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Commands.UpdateItem;

public record UpdateItemCommand(
    Guid ItemId,
    Guid ApartmentId,
    string UserId,
    string? CustomName,
    decimal Quantity
) : IRequest<Result<Unit>>;

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public UpdateItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ApartmentId == request.ApartmentId, cancellationToken);

        if (item == null)
            return Result<Unit>.Failure("Item not found.");

        if (request.Quantity <= 0)
            return Result<Unit>.Failure("Quantity must be greater than zero.");

        if (!string.IsNullOrWhiteSpace(request.CustomName))
            item.CustomName = request.CustomName.Trim();

        item.Quantity = request.Quantity;
        item.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}