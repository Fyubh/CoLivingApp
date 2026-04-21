using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetMyMaintenanceRequests;

public class GetMyMaintenanceRequestsQueryHandler
    : IRequestHandler<GetMyMaintenanceRequestsQuery, Result<List<MaintenanceRequestDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMyMaintenanceRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<MaintenanceRequestDto>>> Handle(
        GetMyMaintenanceRequestsQuery request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<List<MaintenanceRequestDto>>.Failure("Не указан пользователь.");

        // Проекция в DTO сразу в запросе — не тащим Include'ы и связанные коллекции в память.
        // JOIN на StaffAssignment → User делается через левые соединения, т.к. оба поля nullable.
        var items = await (
            from m in _context.MaintenanceRequests
            where m.ReportedByUserId == request.UserId
            orderby m.CreatedAt descending
            join sa in _context.StaffAssignments on m.AssignedStaffAssignmentId equals sa.Id into saGroup
            from sa in saGroup.DefaultIfEmpty()
            join u in _context.Users on sa!.UserId equals u.Id into uGroup
            from u in uGroup.DefaultIfEmpty()
            select new MaintenanceRequestDto(
                m.Id,
                m.Title,
                m.Description,
                m.Category,
                m.Priority,
                m.Status,
                m.PhotoUrl,
                m.CompletionPhotoUrl,
                m.CompletionNotes,
                u != null ? u.Name : null,
                m.CreatedAt,
                m.AssignedAt,
                m.StartedAt,
                m.CompletedAt,
                m.ResidentRating
            )
        ).ToListAsync(ct);

        return Result<List<MaintenanceRequestDto>>.Success(items);
    }
}