using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingFloors;

public record FloorShortDto(Guid Id, int Number, string? Name);

public record GetBuildingFloorsQuery(string AdminUserId, Guid BuildingId)
    : IRequest<Result<List<FloorShortDto>>>;

public class GetBuildingFloorsQueryHandler
    : IRequestHandler<GetBuildingFloorsQuery, Result<List<FloorShortDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingFloorsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<FloorShortDto>>> Handle(
        GetBuildingFloorsQuery request, CancellationToken ct)
    {
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<List<FloorShortDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        var floors = await _context.Floors
            .Where(f => f.BuildingId == request.BuildingId)
            .OrderBy(f => f.Number)
            .Select(f => new FloorShortDto(f.Id, f.Number, f.Name))
            .ToListAsync(ct);

        return Result<List<FloorShortDto>>.Success(floors);
    }
}