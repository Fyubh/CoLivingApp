using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record ConfirmChoreCommand(Guid ChoreId, Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;

public class ConfirmChoreCommandHandler : IRequestHandler<ConfirmChoreCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public ConfirmChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(ConfirmChoreCommand request, CancellationToken cancellationToken)
    {
        // Ищем задачу в базе
        var chore = await _context.Chores.FirstOrDefaultAsync(c => c.Id == request.ChoreId && c.ApartmentId == request.ApartmentId, cancellationToken);
        
        if (chore == null) return Result<Unit>.Failure("Задача не найдена");
        
        // ПРОВЕРКА БЕЗОПАСНОСТИ: Сам себя подтвердить не можешь
        if (chore.AssignedUserId == request.UserId)
            return Result<Unit>.Failure("Вы не можете оценивать собственную задачу.");

        // Всё ок — одобряем работу и закрываем задачу
        chore.Status = ChoreStatus.Completed; 
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}