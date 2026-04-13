using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record GetRecurringChoresQuery(Guid ApartmentId) : IRequest<Result<List<RecurringChoreDto>>>;

public record RecurringChoreDto(Guid Id, string Title, string? AssignedName, int Pattern, int Interval, DateTime NextRunDate);

public class GetRecurringChoresQueryHandler : IRequestHandler<GetRecurringChoresQuery, Result<List<RecurringChoreDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetRecurringChoresQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<RecurringChoreDto>>> Handle(GetRecurringChoresQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _context.RecurringChores
            .AsNoTracking()
            .Include(c => c.AssignedUser)
            .Where(c => c.ApartmentId == request.ApartmentId && c.IsActive)
            .Select(c => new RecurringChoreDto(
                c.Id, c.Title, c.AssignedUser != null ? c.AssignedUser.Name : null, (int)c.Pattern, c.Interval, c.NextRunDate
            )).ToListAsync(cancellationToken);

        return Result<List<RecurringChoreDto>>.Success(schedules);
    }
}