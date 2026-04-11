using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Commands.ConsumeItem;

public record ConsumeItemCommand(Guid ItemId, Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;

public class ConsumeItemCommandHandler : IRequestHandler<ConsumeItemCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public ConsumeItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(ConsumeItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.ItemId && i.ApartmentId == request.ApartmentId, cancellationToken);
        if (item == null) return Result<Unit>.Failure("Товар не найден.");

        item.Status = ItemStatus.Consumed; // Убираем из инвентаря
        item.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}