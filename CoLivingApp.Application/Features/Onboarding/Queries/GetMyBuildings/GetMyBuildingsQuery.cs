using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetMyBuildings;

/// <summary>
/// "Какие здания мне доступны как админу?"
/// Закрывает старый костыль, где BuildingId надо было вводить через prompt.
///
/// Address собирается из двух полей Building.AddressLine + Building.City —
/// фронту удобно показать одной строкой, а отдельно эти поля на этом экране не нужны.
/// </summary>
public record BuildingShortDto(Guid Id, string Name, string Address);

public record GetMyBuildingsQuery(string AdminUserId)
    : IRequest<Result<List<BuildingShortDto>>>;

public class GetMyBuildingsQueryHandler
    : IRequestHandler<GetMyBuildingsQuery, Result<List<BuildingShortDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetMyBuildingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<BuildingShortDto>>> Handle(
        GetMyBuildingsQuery request, CancellationToken ct)
    {
        var buildings = await _context.StaffAssignments
            .Where(s => s.UserId == request.AdminUserId
                        && s.Role == StaffRole.BuildingAdmin
                        && s.IsActive)
            .Select(s => s.Building!)
            .Distinct()
            .Select(b => new BuildingShortDto(
                b.Id,
                b.Name,
                // Конкатенация делается в SQL — EF Core переводит её в expression,
                // если использовать +. ?? тоже SQL-friendly.
                (b.AddressLine ?? string.Empty) + ", " + (b.City ?? string.Empty)))
            .ToListAsync(ct);

        return Result<List<BuildingShortDto>>.Success(buildings);
    }
}