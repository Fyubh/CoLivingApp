// Файл: CoLivingApp.Application/Features/Expenses/Commands/CreateExpense/CreateExpenseCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Commands.CreateExpense;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        // 1. Получаем всех активных жильцов квартиры
        var members = await _context.ApartmentMembers
            .Where(m => m.ApartmentId == request.ApartmentId && m.IsActive)
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
            return Result<Guid>.Failure("В квартире нет активных жильцов.");

        // 2. Создаем основной расход (чек)
        var expense = new Expense
        {
            ApartmentId = request.ApartmentId,
            PayerId = request.PayerId,
            Amount = request.Amount,
            Description = request.Description,
            Date = DateTime.UtcNow
        };

        // 3. Рассчитываем долю каждого (делим поровну)
        decimal splitAmount = request.Amount / members.Count;

        // 4. Создаем записи о долях (кто сколько должен в рамках этого чека)
        foreach (var member in members)
        {
            var split = new ExpenseSplit
            {
                ExpenseId = expense.Id,
                UserId = member.UserId,
                Amount = splitAmount
            };
            expense.Splits.Add(split);
        }

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(expense.Id);
    }
}