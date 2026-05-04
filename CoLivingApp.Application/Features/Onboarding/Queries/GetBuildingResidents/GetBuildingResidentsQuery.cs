using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Onboarding.Queries.GetBuildingResidents;

public record BuildingResidentDto(
    string UserId,
    string Name,
    string Email,
    Guid ApartmentId,
    string ApartmentName,
    string? UnitNumber,
    Guid? RoomId,
    string? RoomNumber,
    DateTime JoinedAt);

public record GetBuildingResidentsQuery(string AdminUserId, Guid BuildingId)
    : IRequest<Result<List<BuildingResidentDto>>>;

public class GetBuildingResidentsQueryHandler
    : IRequestHandler<GetBuildingResidentsQuery, Result<List<BuildingResidentDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingResidentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<BuildingResidentDto>>> Handle(
        GetBuildingResidentsQuery request, CancellationToken ct)
    {
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);

        if (!isAdmin)
            return Result<List<BuildingResidentDto>>.Failure(
                "У вас нет прав администратора в этом здании.");

        var residents = await _context.ApartmentMembers
            .Where(m => m.IsActive && m.Apartment!.BuildingId == request.BuildingId)
            .OrderBy(m => m.Apartment!.UnitNumber)
            .ThenBy(m => m.Room!.Number)
            .ThenBy(m => m.User!.Name)
            .Select(m => new BuildingResidentDto(
                m.UserId,
                m.User!.Name,
                m.User!.Email,
                m.ApartmentId,
                m.Apartment!.Name,
                m.Apartment!.UnitNumber,
                m.RoomId,
                m.Room != null ? m.Room.Number : null,
                m.JoinedAt))
            .ToListAsync(ct);

        return Result<List<BuildingResidentDto>>.Success(residents);
    }
}
