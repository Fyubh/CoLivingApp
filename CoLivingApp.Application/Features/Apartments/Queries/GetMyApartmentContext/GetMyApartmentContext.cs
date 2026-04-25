using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Apartments.Queries.GetMyApartmentContext;

/// <summary>
/// Вернуть контекст проживания юзера — достаточно информации, чтобы форма заявки на ремонт
/// могла показать корректные поля без дополнительных запросов.
/// Если жилец в нескольких квартирах (редкий кейс в roommate-mode), берём последнюю активную —
/// для B2B это всегда одна.
/// </summary>
public record GetMyApartmentContextQuery(string UserId)
    : IRequest<Result<MyApartmentContextDto?>>;

public class GetMyApartmentContextQueryHandler
    : IRequestHandler<GetMyApartmentContextQuery, Result<MyApartmentContextDto?>>
{
    private readonly IApplicationDbContext _context;
    public GetMyApartmentContextQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MyApartmentContextDto?>> Handle(
        GetMyApartmentContextQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<MyApartmentContextDto?>.Failure("Не указан пользователь.");

        // Активное членство, последнее по дате вступления.
        var membership = await _context.ApartmentMembers
            .Where(m => m.UserId == request.UserId && m.IsActive)
            .OrderByDescending(m => m.JoinedAt)
            .Select(m => new { m.ApartmentId })
            .FirstOrDefaultAsync(ct);

        if (membership == null)
            return Result<MyApartmentContextDto?>.Success(null); // жилец ни в одной квартире

        // Загружаем квартиру + здание + комнаты одним батчом.
        var data = await _context.Apartments
            .Where(a => a.Id == membership.ApartmentId)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.UnitNumber,
                a.BuildingId,
                BuildingName = a.Building != null ? a.Building.Name : null,
                Rooms = a.Rooms
                    .OrderBy(r => r.Number)
                    .Select(r => new RoomOptionDto(
                        r.Id,
                        r.Number,
                        r.Type.ToString()))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (data == null)
            return Result<MyApartmentContextDto?>.Failure("Квартира не найдена.");

        var dto = new MyApartmentContextDto(
            data.Id,
            data.Name,
            data.UnitNumber,
            data.BuildingId,
            data.BuildingName,
            data.Rooms);

        return Result<MyApartmentContextDto?>.Success(dto);
    }
}