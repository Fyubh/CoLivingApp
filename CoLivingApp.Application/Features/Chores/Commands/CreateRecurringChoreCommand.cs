using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Chores;

// 1. Добавили Description и Category сюда 👇
public record CreateRecurringChoreCommand(
    Guid ApartmentId, 
    string Title, 
    string? Description, 
    int Category, 
    string? AssignedUserId, 
    RecurrencePattern Pattern, 
    int Interval, 
    DateTime StartDate
) : IRequest<Result<Guid>>;

public class CreateRecurringChoreCommandHandler : IRequestHandler<CreateRecurringChoreCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateRecurringChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateRecurringChoreCommand request, CancellationToken cancellationToken)
    {
        var safeUtcDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);

        var recChore = new RecurringChore
        {
            ApartmentId = request.ApartmentId,
            Title = request.Title,
            Description = request.Description,             // Теперь компилятор это видит
            Category = (ChoreCategory)request.Category,    // И это тоже
            AssignedUserId = string.IsNullOrWhiteSpace(request.AssignedUserId) ? null : request.AssignedUserId,
            Pattern = request.Pattern,
            Interval = request.Interval,
            NextRunDate = safeUtcDate,
            IsActive = true
        };

        _context.RecurringChores.Add(recChore);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(recChore.Id);
    }
}