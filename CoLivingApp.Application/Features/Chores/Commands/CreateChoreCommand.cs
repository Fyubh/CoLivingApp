using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Chores;

public record CreateChoreCommand(Guid ApartmentId, string Title, string? AssignedUserId, DateTime? DueDate) : IRequest<Result<Guid>>;

public class CreateChoreCommandHandler : IRequestHandler<CreateChoreCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateChoreCommand request, CancellationToken cancellationToken)
    {
        DateTime? safeUtcDueDate = request.DueDate.HasValue 
            ? DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc) 
            : null;

        var chore = new Chore
        {
            ApartmentId = request.ApartmentId,
            Title = request.Title,
            AssignedUserId = string.IsNullOrWhiteSpace(request.AssignedUserId) ? null : request.AssignedUserId,
            DueDate = safeUtcDueDate
        };
        
        _context.Chores.Add(chore);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(chore.Id);
    }
}