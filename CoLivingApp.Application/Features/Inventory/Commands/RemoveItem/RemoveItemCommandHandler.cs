using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Commands.RemoveItem;

public class RemoveItemCommandHandler : IRequestHandler<RemoveItemCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public RemoveItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(RemoveItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ApartmentId == request.ApartmentId, cancellationToken);

        if (item == null) return Result<Unit>.Failure("Товар не найден.");

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}