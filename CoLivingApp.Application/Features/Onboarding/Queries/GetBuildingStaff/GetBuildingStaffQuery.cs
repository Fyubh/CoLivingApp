using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingStaff;

/// <summary>
/// Список сотрудников здания.
/// Закрывает старый костыль admin.html, где StaffAssignmentId надо было
/// копировать руками из БД.
/// </summary>
public record StaffShortDto(
    Guid StaffAssignmentId,
    string UserId,
    string Name,
    string Email,
    StaffRole Role,
    ContractorType? Specialization,
    bool IsActive,
    bool IsOnShift,
    int CompletedTasksCount);

public record GetBuildingStaffQuery(string AdminUserId, Guid BuildingId)
    : IRequest<Result<List<StaffShortDto>>>;

public class GetBuildingStaffQueryHandler
    : IRequestHandler<GetBuildingStaffQuery, Result<List<StaffShortDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingStaffQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<StaffShortDto>>> Handle(
        GetBuildingStaffQuery request, CancellationToken ct)
    {
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<List<StaffShortDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        var staff = await _context.StaffAssignments
            .Where(s => s.BuildingId == request.BuildingId)
            .OrderBy(s => s.Role)
            .ThenBy(s => s.User!.Name)
            .Select(s => new StaffShortDto(
                s.Id,
                s.UserId,
                s.User!.Name,
                s.User!.Email,
                s.Role,
                s.Specialization,
                s.IsActive,
                s.IsOnShift,
                s.CompletedTasksCount))
            .ToListAsync(ct);

        return Result<List<StaffShortDto>>.Success(staff);
    }
}