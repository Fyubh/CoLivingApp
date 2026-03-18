using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Commands.MoveToCart;

public class MoveItemToCartCommandHandler : IRequestHandler<MoveItemToCartCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public MoveItemToCartCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(MoveItemToCartCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ApartmentId == request.ApartmentId, cancellationToken);

        if (item == null) return Result<Unit>.Failure("Товар не найден.");

        // Меняем статус на InCart
        item.Status = ItemStatus.InCart;
        item.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}