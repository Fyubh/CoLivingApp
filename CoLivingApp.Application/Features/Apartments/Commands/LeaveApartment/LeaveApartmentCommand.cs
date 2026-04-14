using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Apartments.Commands.LeaveApartment;

public record LeaveApartmentCommand(Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;

public class LeaveApartmentCommandHandler : IRequestHandler<LeaveApartmentCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public LeaveApartmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(LeaveApartmentCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.ApartmentMembers
            .FirstOrDefaultAsync(m => m.ApartmentId == request.ApartmentId
                                      && m.UserId == request.UserId
                                      && m.IsActive, cancellationToken);

        if (member == null)
            return Result<Unit>.Failure("You are not a member of this apartment.");

        // Проверяем, не остается ли квартира совсем без жильцов
        var activeCount = await _context.ApartmentMembers
            .CountAsync(m => m.ApartmentId == request.ApartmentId && m.IsActive, cancellationToken);

        // Помечаем как неактивного (не удаляем — история финансов должна сохраниться)
        member.IsActive = false;
        member.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}