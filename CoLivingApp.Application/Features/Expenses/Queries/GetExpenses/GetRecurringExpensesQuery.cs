using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses;

public record GetRecurringExpensesQuery(Guid ApartmentId) : IRequest<Result<List<RecurringExpenseDto>>>;

public record RecurringExpenseDto(Guid Id, string Description, decimal Amount, int Pattern, int Interval, DateTime NextRunDate, string PayerName);

public class GetRecurringExpensesQueryHandler : IRequestHandler<GetRecurringExpensesQuery, Result<List<RecurringExpenseDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetRecurringExpensesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<RecurringExpenseDto>>> Handle(GetRecurringExpensesQuery request, CancellationToken cancellationToken)
    {
        var subs = await _context.RecurringExpenses
            .AsNoTracking()
            .Include(e => e.Payer)
            .Where(e => e.ApartmentId == request.ApartmentId && e.IsActive)
            .Select(e => new RecurringExpenseDto(
                e.Id, e.Description, e.Amount, (int)e.Pattern, e.Interval, e.NextRunDate, e.Payer != null ? e.Payer.Name : "Unknown"
            )).ToListAsync(cancellationToken);

        return Result<List<RecurringExpenseDto>>.Success(subs);
    }
}