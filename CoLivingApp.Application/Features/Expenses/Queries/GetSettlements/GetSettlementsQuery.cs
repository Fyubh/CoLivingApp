using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Expenses.Queries.GetSettlements;

public record GetSettlementsQuery(Guid ApartmentId) : IRequest<Result<List<SettlementDto>>>;

public record SettlementDto(Guid Id, string SenderName, string ReceiverName, decimal Amount, DateTime Date);

public class GetSettlementsQueryHandler : IRequestHandler<GetSettlementsQuery, Result<List<SettlementDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetSettlementsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<SettlementDto>>> Handle(GetSettlementsQuery request, CancellationToken cancellationToken)
    {
        var settlements = await _context.Settlements
            .AsNoTracking()
            .Include(s => s.Sender)
            .Include(s => s.Receiver)
            .Where(s => s.ApartmentId == request.ApartmentId)
            .OrderByDescending(s => s.Date)
            .Select(s => new SettlementDto(
                s.Id,
                s.Sender != null ? s.Sender.Name : "Unknown",
                s.Receiver != null ? s.Receiver.Name : "Unknown",
                s.Amount,
                s.Date
            ))
            .ToListAsync(cancellationToken);

        return Result<List<SettlementDto>>.Success(settlements);
    }
}