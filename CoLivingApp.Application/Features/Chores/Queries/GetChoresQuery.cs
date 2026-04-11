using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

// ДОБАВИЛИ CurrentUserId
public record GetChoresQuery(Guid ApartmentId, string CurrentUserId) : IRequest<Result<List<ChoreDto>>>;

// ДОБАВИЛИ CanComplete и CanReview
public record ChoreDto(Guid Id, string Title, int Status, DateTime? DueDate, string? AssignedName, bool CanComplete, bool CanReview);

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
            .OrderBy(c => c.Status) 
            .ThenBy(c => c.DueDate ?? DateTime.MaxValue) 
            .ToListAsync(cancellationToken);

        var dtos = chores.Select(c => new ChoreDto(
            c.Id, 
            c.Title, 
            (int)c.Status,
            c.DueDate,
            c.AssignedUser != null ? c.AssignedUser.Name : null,
            // ЛОГИКА ДОСТУПА ЗДЕСЬ:
            CanComplete: c.Status == ChoreStatus.Pending && (c.AssignedUserId == null || c.AssignedUserId == request.CurrentUserId),
            CanReview: c.Status == ChoreStatus.NeedsReview && c.AssignedUserId != request.CurrentUserId
        )).ToList();

        return Result<List<ChoreDto>>.Success(dtos);
    }
}