using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Queries.GetBalance;

// Запрос и DTO в одном месте
public record GetBalanceQuery(Guid ApartmentId, string CurrentUserId) : IRequest<Result<List<UserBalanceDto>>>;
public record UserBalanceDto(string UserId, string UserName, decimal Balance);

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, Result<List<UserBalanceDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBalanceQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<UserBalanceDto>>> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        // Получаем всех соседей (КРОМЕ НАС САМИХ)
        var roommates = await _context.ApartmentMembers
            .Where(m => m.ApartmentId == request.ApartmentId && m.IsActive && m.UserId != request.CurrentUserId)
            .Include(m => m.User)
            .ToListAsync(cancellationToken);

        var balances = new List<UserBalanceDto>();

        foreach (var roomie in roommates)
        {
            var owedToMe = await _context.ExpenseSplits.Where(s => s.Expense!.ApartmentId == request.ApartmentId && s.Expense.PayerId == request.CurrentUserId && s.UserId == roomie.UserId).SumAsync(s => s.Amount, cancellationToken);
            var iOwe = await _context.ExpenseSplits.Where(s => s.Expense!.ApartmentId == request.ApartmentId && s.Expense.PayerId == roomie.UserId && s.UserId == request.CurrentUserId).SumAsync(s => s.Amount, cancellationToken);
            var receivedFromRoomie = await _context.Settlements.Where(s => s.ApartmentId == request.ApartmentId && s.SenderId == roomie.UserId && s.ReceiverId == request.CurrentUserId).SumAsync(s => s.Amount, cancellationToken);
            var sentToRoomie = await _context.Settlements.Where(s => s.ApartmentId == request.ApartmentId && s.SenderId == request.CurrentUserId && s.ReceiverId == roomie.UserId).SumAsync(s => s.Amount, cancellationToken);

            decimal finalBalance = (owedToMe - iOwe) + (sentToRoomie - receivedFromRoomie);

            balances.Add(new UserBalanceDto(roomie.UserId, roomie.User?.Name ?? "Unknown", finalBalance));
        }
        return Result<List<UserBalanceDto>>.Success(balances);
    }
}