using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetBuildingMaintenanceRequests;

/// <summary>
/// Заявки по зданию для админа. Фильтры — Status и Priority, обе опциональные.
/// Авторизация: RequestingUserId должен быть активным BuildingAdmin этого здания.
/// </summary>
public record GetBuildingMaintenanceRequestsQuery(
    string RequestingUserId,
    Guid BuildingId,
    MaintenanceStatus? Status = null,
    MaintenancePriority? Priority = null
) : IRequest<Result<List<BuildingMaintenanceRequestDto>>>;

public class GetBuildingMaintenanceRequestsQueryHandler
    : IRequestHandler<GetBuildingMaintenanceRequestsQuery, Result<List<BuildingMaintenanceRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingMaintenanceRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<BuildingMaintenanceRequestDto>>> Handle(
        GetBuildingMaintenanceRequestsQuery request, CancellationToken ct)
    {
        // Авторизация.
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.RequestingUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<List<BuildingMaintenanceRequestDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        var query = _context.MaintenanceRequests
            .Where(m => m.BuildingId == request.BuildingId);

        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        if (request.Priority.HasValue)
            query = query.Where(m => m.Priority == request.Priority.Value);

        // Сортировка: открытые сначала (по приоритету потом по дате), закрытые внизу.
        // Для MVP делаем просто по дате создания descending.
        var items = await (
            from m in query.OrderByDescending(x => x.CreatedAt)
            join reporter in _context.Users on m.ReportedByUserId equals reporter.Id
            join apt in _context.Apartments on m.ApartmentId equals apt.Id into aptGroup
            from apt in aptGroup.DefaultIfEmpty()
            join room in _context.Rooms on m.RoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            join sa in _context.StaffAssignments on m.AssignedStaffAssignmentId equals sa.Id into saGroup
            from sa in saGroup.DefaultIfEmpty()
            join staff in _context.Users on sa!.UserId equals staff.Id into staffGroup
            from staff in staffGroup.DefaultIfEmpty()
            select new BuildingMaintenanceRequestDto(
                m.Id,
                m.Title,
                m.Description,
                m.Category,
                m.Priority,
                m.Status,
                reporter.Name,
                apt != null ? apt.UnitNumber : null,
                room != null ? room.Number : null,
                m.PhotoUrl,
                staff != null ? staff.Name : null,
                m.CreatedAt,
                m.CompletedAt,
                m.ResidentRating
            )
        ).ToListAsync(ct);

        return Result<List<BuildingMaintenanceRequestDto>>.Success(items);
    }
}