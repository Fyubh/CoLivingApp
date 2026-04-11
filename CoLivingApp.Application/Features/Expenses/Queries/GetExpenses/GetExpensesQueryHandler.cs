using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Queries.GetExpenses;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<List<ExpenseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetExpensesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Payer)
            .Where(e => e.ApartmentId == request.ApartmentId)
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseDto(
                e.Id, 
                e.Description, 
                e.Amount, 
                e.Date, 
                e.PayerId, 
                e.Payer != null ? e.Payer.Name : "Unknown", 
                (int)e.Category
            ))
            .ToListAsync(cancellationToken); // <--- ВОТ ЗДЕСЬ БЫЛА ОШИБКА, ЭТОГО КУСКА НЕ ХВАТАЛО

        return Result<List<ExpenseDto>>.Success(expenses);
    }
}