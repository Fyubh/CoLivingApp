// Файл: CoLivingApp.Application/Features/Expenses/Queries/GetBalance/GetBalanceQuery.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Queries.GetBalance;

public record GetBalanceQuery(Guid ApartmentId) : IRequest<Result<List<UserBalanceDto>>>;

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, Result<List<UserBalanceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBalanceQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<UserBalanceDto>>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        // 1. Получаем список всех жильцов квартиры
        var members = await _context.ApartmentMembers
            .Where(m => m.ApartmentId == request.ApartmentId && m.IsActive)
            .Include(m => m.User)
            .ToListAsync(cancellationToken);

        var balances = new List<UserBalanceDto>();

        foreach (var member in members)
        {
            // 2. Сколько ДРУГИЕ должны этому юзеру (Сплиты в его чеках, где UserId не равен его ID)
            var owedToHim = await _context.ExpenseSplits
                .Where(s => s.Expense!.ApartmentId == request.ApartmentId && 
                            s.Expense.PayerId == member.UserId && 
                            s.UserId != member.UserId)
                .SumAsync(s => s.Amount, cancellationToken);

            // 3. Сколько ЭТОТ юзер должен другим (Его сплиты в чужих чеках)
            var heOwes = await _context.ExpenseSplits
                .Where(s => s.Expense!.ApartmentId == request.ApartmentId && 
                            s.Expense.PayerId != member.UserId && 
                            s.UserId == member.UserId)
                .SumAsync(s => s.Amount, cancellationToken);

            // Итоговый баланс
            balances.Add(new UserBalanceDto(
                member.UserId,
                member.User?.Name ?? "Unknown",
                owedToHim - heOwes
            ));
        }

        return Result<List<UserBalanceDto>>.Success(balances);
    }
}