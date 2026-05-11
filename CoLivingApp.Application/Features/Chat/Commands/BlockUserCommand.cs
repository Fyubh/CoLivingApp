using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Блокировка жильца. GetChatHistoryQuery после этого не вернёт сообщения
/// BlockedUserId со стороны Blocker'а. Идемпотентно по unique index.
/// </summary>
public record BlockUserCommand(string BlockerId, string BlockedUserId) : IRequest<Result<bool>>;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public BlockUserCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BlockedUserId))
            return Result<bool>.Failure("Не указан пользователь.");

        if (request.BlockerId == request.BlockedUserId)
            return Result<bool>.Failure("Нельзя заблокировать самого себя.");

        var exists = await _context.ChatBlocks.AnyAsync(b =>
            b.BlockerId == request.BlockerId && b.BlockedUserId == request.BlockedUserId,
            cancellationToken);

        if (exists)
            return Result<bool>.Success(true); // idempotent

        _context.ChatBlocks.Add(new ChatBlock
        {
            BlockerId = request.BlockerId,
            BlockedUserId = request.BlockedUserId
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
