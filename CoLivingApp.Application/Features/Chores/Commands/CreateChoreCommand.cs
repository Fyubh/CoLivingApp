using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums; // <--- ВОТ ЭТА ВАЖНАЯ СТРОКА
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Chores;

// 1. Обновили рекорд (добавили Description и Category)
public record CreateChoreCommand(
    Guid ApartmentId, 
    string Title, 
    string? Description, 
    int Category, 
    string? AssignedUserId, 
    DateTime? DueDate
) : IRequest<Result<Guid>>;

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
            Description = request.Description,             // Тот самый кусок
            Category = (ChoreCategory)request.Category,    // Тот самый кусок
            AssignedUserId = string.IsNullOrWhiteSpace(request.AssignedUserId) ? null : request.AssignedUserId,
            DueDate = safeUtcDueDate
        };
        
        _context.Chores.Add(chore);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<Guid>.Success(chore.Id);
    }
}