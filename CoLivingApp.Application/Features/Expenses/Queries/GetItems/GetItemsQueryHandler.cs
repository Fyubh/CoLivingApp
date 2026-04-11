using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Inventory.Queries.GetItems;

public record GetItemsQuery(Guid ApartmentId, ItemStatus Status) : IRequest<Result<List<ItemDto>>>;

public record ItemDto(Guid Id, string Name, decimal Quantity, int Unit, int Category, int Location, DateTime? ExpiryDate);

public class GetItemsQueryHandler : IRequestHandler<GetItemsQuery, Result<List<ItemDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetItemsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ItemDto>>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        // 1. Достаем сущности из БД (без трансформаций, чтобы PostgreSQL не ругался)
        var items = await _context.InventoryItems
            .AsNoTracking()
            .Where(i => i.ApartmentId == request.ApartmentId && i.Status == request.Status)
            .OrderBy(i => i.ExpiryDate) // Сортируем по дате
            .ToListAsync(cancellationToken);

        // 2. Превращаем в DTO уже в оперативной памяти C#
        var dtos = items.Select(i => new ItemDto(
            i.Id, 
            i.CustomName ?? "Unknown", 
            i.Quantity, 
            (int)i.Unit, 
            (int)i.Category, 
            (int)i.Location, 
            i.ExpiryDate
        )).ToList();

        return Result<List<ItemDto>>.Success(dtos);
    }
}