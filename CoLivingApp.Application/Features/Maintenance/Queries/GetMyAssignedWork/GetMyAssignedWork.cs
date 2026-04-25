using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Queries.GetAvailableWork;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetMyAssignedWork;

/// <summary>
/// "Что мне уже назначили?" — задачи в работе у подрядчика.
/// Показываем только Assigned и InProgress (Completed/Cancelled он может посмотреть в истории отдельно).
/// Переиспользуем WorkItemDto из GetAvailableWork — структура та же самая.
/// </summary>
public record GetMyAssignedWorkQuery(string UserId)
    : IRequest<Result<List<WorkItemDto>>>;

public class GetMyAssignedWorkQueryHandler
    : IRequestHandler<GetMyAssignedWorkQuery, Result<List<WorkItemDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetMyAssignedWorkQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<WorkItemDto>>> Handle(
        GetMyAssignedWorkQuery request, CancellationToken ct)
    {
        // ID всех активных assignments юзера — через них и связаны заявки.
        var myAssignmentIds = await _context.StaffAssignments
            .Where(s => s.UserId == request.UserId && s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (myAssignmentIds.Count == 0)
            return Result<List<WorkItemDto>>.Success(new List<WorkItemDto>());

        var items = await (
            from m in _context.MaintenanceRequests
            where m.AssignedStaffAssignmentId != null
                  && myAssignmentIds.Contains(m.AssignedStaffAssignmentId.Value)
                  && (m.Status == MaintenanceStatus.Assigned
                      || m.Status == MaintenanceStatus.InProgress)
            join b in _context.Buildings on m.BuildingId equals b.Id
            join apt in _context.Apartments on m.ApartmentId equals apt.Id into aptGroup
            from apt in aptGroup.DefaultIfEmpty()
            join room in _context.Rooms on m.RoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            orderby (int)m.Priority descending, m.AssignedAt
            select new WorkItemDto(
                m.Id,
                m.Title,
                m.Description,
                m.Category,
                m.Priority,
                m.Status,
                m.PhotoUrl,
                b.Name,
                b.AddressLine,
                apt != null ? apt.UnitNumber : null,
                room != null ? room.Number : null,
                m.CreatedAt,
                m.AssignedAt,
                m.StartedAt
            )
        ).ToListAsync(ct);

        return Result<List<WorkItemDto>>.Success(items);
    }
}