using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Queries;

public record GetChatHistoryQuery(Guid ApartmentId, string RequestingUserId) : IRequest<Result<List<ChatMessageDto>>>;

/// <summary>
/// Сообщение чата для клиента. `IsDeleted=true` означает что автор удалил —
/// `Text` будет пустым, iOS подставит «[Сообщение удалено]». Сохраняем
/// сообщение в истории (а не выпиливаем из выборки), чтобы индексы и репорты
/// оставались валидными.
/// </summary>
public record ChatMessageDto(Guid Id, string SenderId, string SenderName, string Text, DateTime SentAt, bool IsDeleted);

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Result<List<ChatMessageDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetChatHistoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ChatMessageDto>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        // Запрашивающий должен быть жильцом этой квартиры.
        var isMember = await _context.ApartmentMembers.AnyAsync(m =>
            m.ApartmentId == request.ApartmentId
            && m.UserId == request.RequestingUserId
            && m.IsActive, cancellationToken);

        if (!isMember)
            return Result<List<ChatMessageDto>>.Failure("Нет доступа к этому чату.");

        var blockedIds = await _context.ChatBlocks
            .Where(b => b.BlockerId == request.RequestingUserId)
            .Select(b => b.BlockedUserId)
            .ToListAsync(cancellationToken);

        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ApartmentId == request.ApartmentId)
            .Where(m => !blockedIds.Contains(m.SenderId))
            .OrderBy(m => m.SentAt)
            .Take(50)
            .Select(m => new ChatMessageDto(
                m.Id,
                m.SenderId,
                m.Sender != null ? m.Sender.Name : "Неизвестный",
                m.IsDeleted ? string.Empty : m.Text,
                m.SentAt,
                m.IsDeleted))
            .ToListAsync(cancellationToken);

        return Result<List<ChatMessageDto>>.Success(messages);
    }
}
