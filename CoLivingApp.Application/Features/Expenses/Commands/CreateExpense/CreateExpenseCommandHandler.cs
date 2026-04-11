using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateExpenseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        // 1. Ищем всех жильцов
        var members = await _context.ApartmentMembers
            .Where(m => m.ApartmentId == request.ApartmentId && m.IsActive)
            .ToListAsync(cancellationToken);

        // Защита: Если человек живет один, чек не делится, выдаем ошибку или предупреждение
        if (members.Count <= 1)
            return Result<Guid>.Failure("Вы единственный жилец в квартире. Делить счет не с кем!");

        var expense = new Expense
        {
            ApartmentId = request.ApartmentId,
            PayerId = request.PayerId,
            Amount = request.Amount,
            Description = request.Description,
            Category = request.Category,
            Date = DateTime.UtcNow
        };

        // Делим на всех
        decimal splitAmount = request.Amount / members.Count;
        foreach (var member in members)
        {
            expense.Splits.Add(new ExpenseSplit { ExpenseId = expense.Id, UserId = member.UserId, Amount = splitAmount });
        }

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(expense.Id);
    }
}