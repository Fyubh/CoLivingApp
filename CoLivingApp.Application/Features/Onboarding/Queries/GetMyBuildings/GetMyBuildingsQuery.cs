using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetMyBuildings;

/// <summary>
/// "Какие здания мне доступны как админу?"
///
/// Admin      — только привязанные через активный StaffAssignment (BuildingAdmin).
/// SuperAdmin — ВСЕ активные здания в системе. SuperAdmin создаёт здания,
///              CreateBuildingCommand сразу же добавляет ему BuildingAdmin-assignment,
///              так что обычно у него тоже есть assignments. Но мы всё равно показываем
///              ему все здания — на случай, когда другой SuperAdmin создал здание раньше.
/// </summary>
public record GetMyBuildingsQuery(string AdminUserId)
    : IRequest<Result<List<BuildingDto>>>;

public class GetMyBuildingsQueryHandler
    : IRequestHandler<GetMyBuildingsQuery, Result<List<BuildingDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetMyBuildingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<BuildingDto>>> Handle(
        GetMyBuildingsQuery request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AdminUserId, ct);

        if (user == null)
            return Result<List<BuildingDto>>.Failure("Пользователь не найден.");

        if (user.Role == UserRole.SuperAdmin)
        {
            var all = await _context.Buildings
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new BuildingDto(b.Id, b.Name, b.City, b.Country, b.IsActive))
                .ToListAsync(ct);

            return Result<List<BuildingDto>>.Success(all);
        }

        var buildings = await _context.StaffAssignments
            .Where(s => s.UserId == request.AdminUserId
                        && s.Role == StaffRole.BuildingAdmin
                        && s.IsActive)
            .Select(s => s.Building!)
            .Where(b => b.IsActive)
            .Distinct()
            .OrderBy(b => b.Name)
            .Select(b => new BuildingDto(b.Id, b.Name, b.City, b.Country, b.IsActive))
            .ToListAsync(ct);

        return Result<List<BuildingDto>>.Success(buildings);
    }
}
