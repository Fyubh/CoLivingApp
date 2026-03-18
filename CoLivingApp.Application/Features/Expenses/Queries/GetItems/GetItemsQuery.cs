// Файл: CoLivingApp.Application/Features/Inventory/Queries/GetItems/GetItemsQuery.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Queries.GetItems;

public record GetItemsQuery(Guid ApartmentId, ItemStatus Status) : IRequest<Result<List<InventoryItem>>>;

public class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, Result<List<InventoryItem>>>
{
    private readonly IApplicationDbContext _context;

    public GetItemsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<InventoryItem>>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.InventoryItems
            .Where(i => i.ApartmentId == request.ApartmentId && i.Status == request.Status)
            .OrderByDescending(i => i.PurchasedAt)
            .ToListAsync(cancellationToken);

        return Result<List<InventoryItem>>.Success(items);
    }
}