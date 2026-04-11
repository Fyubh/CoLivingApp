using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chores;

public record CompleteChoreCommand(Guid ChoreId, Guid ApartmentId) : IRequest<Result<Unit>>;

public class CompleteChoreCommandHandler : IRequestHandler<CompleteChoreCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    public CompleteChoreCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Unit>> Handle(CompleteChoreCommand request, CancellationToken cancellationToken)
    {
        var chore = await _context.Chores.FirstOrDefaultAsync(c => c.Id == request.ChoreId && c.ApartmentId == request.ApartmentId, cancellationToken);
        if (chore == null) return Result<Unit>.Failure("Задача не найдена");

        chore.IsCompleted = true; // Отмечаем как выполненную
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}