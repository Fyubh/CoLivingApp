using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Soft-delete своего сообщения. Только автор сообщения может удалить — это
/// требование App Review для UGC-поверхностей (пользователь должен иметь
/// возможность убрать собственный пост). Чужие сообщения через эту команду
/// удалить нельзя — для них есть Report.
/// </summary>
public record DeleteMessageCommand(Guid MessageId, string RequestingUserId) : IRequest<Result<bool>>;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public DeleteMessageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<bool>.Failure("Сообщение не найдено.");

        if (message.SenderId != request.RequestingUserId)
            return Result<bool>.Failure("Можно удалить только своё сообщение.");

        if (message.IsDeleted)
            return Result<bool>.Success(true); // idempotent

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.Text = string.Empty; // не утекаем содержимое после удаления

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
