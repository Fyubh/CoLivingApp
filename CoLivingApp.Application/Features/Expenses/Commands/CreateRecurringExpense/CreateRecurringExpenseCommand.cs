using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Commands.CreateRecurringExpense;

public record CreateRecurringExpenseCommand(
    Guid ApartmentId, string PayerId, decimal Amount, string Description, 
    ExpenseCategory Category, RecurrencePattern Pattern, int Interval, DateTime StartDate
) : IRequest<Result<Guid>>;

public class CreateRecurringExpenseCommandHandler : IRequestHandler<CreateRecurringExpenseCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateRecurringExpenseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateRecurringExpenseCommand request, CancellationToken cancellationToken)
    {
        var membersCount = await _context.ApartmentMembers
            .CountAsync(m => m.ApartmentId == request.ApartmentId && m.IsActive, cancellationToken);

        if (membersCount <= 1)
            return Result<Guid>.Failure("Вы единственный жилец. Регулярные счета не с кем делить.");

        var safeUtcDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);

        var recExpense = new RecurringExpense
        {
            ApartmentId = request.ApartmentId,
            PayerId = request.PayerId,
            Amount = request.Amount,
            Description = request.Description,
            Category = request.Category,
            Pattern = request.Pattern,
            Interval = request.Interval,
            NextRunDate = safeUtcDate,
            IsActive = true
        };

        _context.RecurringExpenses.Add(recExpense);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(recExpense.Id);
    }
}