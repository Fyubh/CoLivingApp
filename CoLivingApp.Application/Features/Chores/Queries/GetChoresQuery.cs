using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record GetChoresQuery(Guid ApartmentId) : IRequest<Result<List<ChoreDto>>>;
public record ChoreDto(Guid Id, string Title, bool IsCompleted, string? AssignedName);

public class GetChoresQueryHandler : IRequestHandler<GetChoresQuery, Result<List<ChoreDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetChoresQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ChoreDto>>> Handle(GetChoresQuery request, CancellationToken cancellationToken)
    {
        var chores = await _context.Chores
            .AsNoTracking()
            .Include(c => c.AssignedUser)
            .Where(c => c.ApartmentId == request.ApartmentId)
            .OrderBy(c => c.IsCompleted) // Сначала невыполненные
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new ChoreDto(c.Id, c.Title, c.IsCompleted, c.AssignedUser != null ? c.AssignedUser.Name : null))
            .ToListAsync(cancellationToken);

        return Result<List<ChoreDto>>.Success(chores);
    }
}