using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Expenses.Commands.SettleDebt;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Finance.Commands.SettleDebt;

public class SettleDebtCommandHandler : IRequestHandler<SettleDebtCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public SettleDebtCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(SettleDebtCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0) return Result<Guid>.Failure("Сумма должна быть больше нуля.");

        var settlement = new Settlement
        {
            ApartmentId = request.ApartmentId,
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            Amount = request.Amount,
            Date = DateTime.UtcNow
        };

        _context.Settlements.Add(settlement);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(settlement.Id);
    }
}