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
        // 1. Сначала ДОСТАЕМ ИЗ БАЗЫ (без Select, чтобы PostgreSQL не ругался на конвертацию)
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Payer)
            .Where(e => e.ApartmentId == request.ApartmentId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

        // 2. Делаем маппинг в DTO уже в оперативной памяти C#
        var dtos = expenses.Select(e => new ExpenseDto(
            e.Id, 
            e.Description, 
            e.Amount, 
            e.Date, 
            e.PayerId, 
            e.Payer != null ? e.Payer.Name : "Unknown", 
            (int)e.Category // В C# это сработает идеально
        )).ToList();

        return Result<List<ExpenseDto>>.Success(dtos);
    }
}