using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Снятие блокировки. Идемпотентно: отсутствие записи трактуется как success.
/// </summary>
public record UnblockUserCommand(string BlockerId, string BlockedUserId) : IRequest<Result<bool>>;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public UnblockUserCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var block = await _context.ChatBlocks.FirstOrDefaultAsync(b =>
            b.BlockerId == request.BlockerId && b.BlockedUserId == request.BlockedUserId,
            cancellationToken);

        if (block == null)
            return Result<bool>.Success(true); // idempotent

        _context.ChatBlocks.Remove(block);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
