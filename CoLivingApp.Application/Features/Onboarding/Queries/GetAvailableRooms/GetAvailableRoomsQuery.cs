using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetAvailableRooms;

/// <summary>
/// Свободная (или частично свободная) комната, в которую админ может заселить нового жильца.
/// Возвращается с denormalized-полями про квартиру/этаж — чтобы фронт мог
/// показать в dropdown'е читаемый текст вроде "Studio 216 / Room A (этаж 2) — 0/1".
/// </summary>
public record AvailableRoomDto(
    Guid RoomId,
    string RoomNumber,
    RoomType Type,
    int CurrentOccupants,
    int MaxOccupancy,
    Guid ApartmentId,
    string ApartmentName,
    string? UnitNumber,
    int FloorNumber);

public record GetAvailableRoomsQuery(string AdminUserId, Guid BuildingId)
    : IRequest<Result<List<AvailableRoomDto>>>;

public class GetAvailableRoomsQueryHandler
    : IRequestHandler<GetAvailableRoomsQuery, Result<List<AvailableRoomDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetAvailableRoomsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<AvailableRoomDto>>> Handle(
        GetAvailableRoomsQuery request, CancellationToken ct)
    {
        // Авторизация
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<List<AvailableRoomDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        // Все комнаты здания, не Maintenance, со счётчиком активных жильцов.
        // GroupJoin даёт count даже если в комнате никто не живёт.
        var rawRooms = await _context.Rooms
            .Where(r => r.Apartment!.BuildingId == request.BuildingId
                     && r.Status != RoomStatus.Maintenance)
            .Select(r => new
            {
                r.Id,
                r.Number,
                r.Type,
                r.MaxOccupancy,
                ApartmentId = r.ApartmentId,
                ApartmentName = r.Apartment!.Name,
                UnitNumber = r.Apartment!.UnitNumber,
                FloorNumber = r.Apartment!.Floor!.Number,
                CurrentOccupants = _context.ApartmentMembers
                    .Count(m => m.RoomId == r.Id && m.IsActive)
            })
            .ToListAsync(ct);

        // Отфильтруем те, где ещё есть место.
        var rooms = rawRooms
            .Where(x => x.CurrentOccupants < x.MaxOccupancy)
            .OrderBy(x => x.FloorNumber)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Number)
            .Select(x => new AvailableRoomDto(
                x.Id, x.Number, x.Type,
                x.CurrentOccupants, x.MaxOccupancy,
                x.ApartmentId, x.ApartmentName, x.UnitNumber,
                x.FloorNumber))
            .ToList();

        return Result<List<AvailableRoomDto>>.Success(rooms);
    }
}