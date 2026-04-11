using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record CompleteChoreCommand(Guid ChoreId, Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;

public class CompleteChoreCommandHandler : IRequestHandler<CompleteChoreCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public CompleteChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(CompleteChoreCommand request, CancellationToken cancellationToken)
    {
        var chore = await _context.Chores.FirstOrDefaultAsync(c => c.Id == request.ChoreId && c.ApartmentId == request.ApartmentId, cancellationToken);
        if (chore == null) return Result<Unit>.Failure("Задача не найдена");

        // ПРОВЕРКА 1: Чужую задачу закрыть нельзя
        if (chore.AssignedUserId != null && chore.AssignedUserId != request.UserId)
            return Result<Unit>.Failure("Эту задачу может выполнить только тот, кому она назначена.");

        // Если задача была "для всех", закрепляем её за тем, кто её сделал
        if (chore.AssignedUserId == null)
            chore.AssignedUserId = request.UserId;

        chore.Status = ChoreStatus.NeedsReview; 
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}