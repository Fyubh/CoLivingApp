using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingApartments;

public record RoomInApartmentDto(
    Guid RoomId,
    string Number,
    RoomType Type,
    int CurrentOccupants,
    int MaxOccupancy);

public record ApartmentInBuildingDto(
    Guid ApartmentId,
    string Name,
    string? UnitNumber,
    int FloorNumber,
    List<RoomInApartmentDto> Rooms);

public record GetBuildingApartmentsQuery(string AdminUserId, Guid BuildingId)
    : IRequest<Result<List<ApartmentInBuildingDto>>>;

public class GetBuildingApartmentsQueryHandler
    : IRequestHandler<GetBuildingApartmentsQuery, Result<List<ApartmentInBuildingDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingApartmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ApartmentInBuildingDto>>> Handle(
        GetBuildingApartmentsQuery request, CancellationToken ct)
    {
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<List<ApartmentInBuildingDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        // Берём плоский список (apartmentId, room, occupants), потом группируем на клиенте.
        // Group + correlated subquery в EF Core надёжнее в плоском виде.
        var rawRows = await _context.Apartments
            .Where(a => a.BuildingId == request.BuildingId)
            .SelectMany(a => a.Rooms.Select(r => new
            {
                ApartmentId = a.Id,
                ApartmentName = a.Name,
                a.UnitNumber,
                FloorNumber = a.Floor!.Number,
                RoomId = r.Id,
                RoomNumber = r.Number,
                RoomType = r.Type,
                MaxOccupancy = r.MaxOccupancy,
                CurrentOccupants = _context.ApartmentMembers
                    .Count(m => m.RoomId == r.Id && m.IsActive)
            }))
            .ToListAsync(ct);

        // Группируем in-memory: один Apartment → много Rooms.
        var grouped = rawRows
            .GroupBy(x => new { x.ApartmentId, x.ApartmentName, x.UnitNumber, x.FloorNumber })
            .OrderBy(g => g.Key.FloorNumber)
            .ThenBy(g => g.Key.UnitNumber)
            .Select(g => new ApartmentInBuildingDto(
                g.Key.ApartmentId,
                g.Key.ApartmentName,
                g.Key.UnitNumber,
                g.Key.FloorNumber,
                g.OrderBy(r => r.RoomNumber)
                 .Select(r => new RoomInApartmentDto(
                    r.RoomId, r.RoomNumber, r.RoomType,
                    r.CurrentOccupants, r.MaxOccupancy))
                 .ToList()))
            .ToList();

        return Result<List<ApartmentInBuildingDto>>.Success(grouped);
    }
}