using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record RejectChoreCommand(Guid ChoreId, Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;

public class RejectChoreCommandHandler : IRequestHandler<RejectChoreCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public RejectChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(RejectChoreCommand request, CancellationToken cancellationToken)
    {
        // Ищем задачу в базе
        var chore = await _context.Chores.FirstOrDefaultAsync(c => c.Id == request.ChoreId && c.ApartmentId == request.ApartmentId, cancellationToken);
        
        if (chore == null) return Result<Unit>.Failure("Задача не найдена");
        
        // ПРОВЕРКА БЕЗОПАСНОСТИ: Сам себе работу не возвращаешь
        if (chore.AssignedUserId == request.UserId)
            return Result<Unit>.Failure("Вы не можете оценивать собственную задачу.");
        
        // Отклоняем: возвращаем задачу обратно в статус "Ожидает выполнения"
        chore.Status = ChoreStatus.Pending; 
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}