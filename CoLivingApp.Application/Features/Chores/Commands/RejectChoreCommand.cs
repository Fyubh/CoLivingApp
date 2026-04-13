using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
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
        var chore = await _context.Chores.FirstOrDefaultAsync(c => c.Id == request.ChoreId && c.ApartmentId == request.ApartmentId, cancellationToken);
        
        if (chore == null) return Result<Unit>.Failure("Задача не найдена");
        if (chore.AssignedUserId == null) return Result<Unit>.Failure("Невозможно оштрафовать: задача никому не назначена");

        // 1. Возвращаем статус в работу
        chore.Status = ChoreStatus.Pending; 
        
        // 2. --- СИСТЕМА ШТРАФОВ ---
        // Сосед, который проверял (request.UserId), получает компенсацию $5.
        // Провинившийся (chore.AssignedUserId) теперь должен ему эти $5.
        var penaltyAmount = 5.0m;
        
        var penalty = new Expense
        {
            ApartmentId = chore.ApartmentId,
            PayerId = request.UserId, // Кому пойдет штраф
            Amount = penaltyAmount,
            Description = $"Штраф за плохую уборку: {chore.Title}",
            Category = ExpenseCategory.Other,
            Date = DateTime.UtcNow
        };
        
        // Вешаем весь долг на исполнителя
        penalty.Splits.Add(new ExpenseSplit 
        { 
            ExpenseId = penalty.Id, 
            UserId = chore.AssignedUserId, 
            Amount = penaltyAmount 
        });

        _context.Expenses.Add(penalty);
        // ------------------------

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}