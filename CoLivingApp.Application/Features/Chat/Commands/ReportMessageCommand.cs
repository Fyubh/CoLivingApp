using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Создаёт жалобу на сообщение. Идемпотентность через unique index
/// (MessageId, ReporterId) — повторный report от того же жильца обрабатывается
/// как успех без ошибки. Автор сообщения не может зарепортить сам себя.
/// </summary>
public record ReportMessageCommand(Guid MessageId, string ReporterId, string? Reason) : IRequest<Result<bool>>;

public class ReportMessageCommandHandler : IRequestHandler<ReportMessageCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public ReportMessageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(ReportMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return Result<bool>.Failure("Сообщение не найдено.");

        if (message.SenderId == request.ReporterId)
            return Result<bool>.Failure("Нельзя пожаловаться на собственное сообщение.");

        // Жалобу принимаем только от жильца этой же квартиры — иначе любой
        // юзер из другой квартиры мог бы засыпать чужой чат репортами.
        var isMember = await _context.ApartmentMembers.AnyAsync(m =>
            m.ApartmentId == message.ApartmentId
            && m.UserId == request.ReporterId
            && m.IsActive, cancellationToken);

        if (!isMember)
            return Result<bool>.Failure("Нет доступа к этому чату.");

        var alreadyReported = await _context.ChatMessageReports.AnyAsync(r =>
            r.MessageId == request.MessageId && r.ReporterId == request.ReporterId,
            cancellationToken);

        if (alreadyReported)
            return Result<bool>.Success(true); // idempotent

        _context.ChatMessageReports.Add(new ChatMessageReport
        {
            MessageId = request.MessageId,
            ReporterId = request.ReporterId,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
