using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Queries;

public record GetChatHistoryQuery(Guid ApartmentId) : IRequest<Result<List<ChatMessageDto>>>;

public record ChatMessageDto(Guid Id, string SenderId, string SenderName, string Text, DateTime SentAt);

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Result<List<ChatMessageDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetChatHistoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ChatMessageDto>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ApartmentId == request.ApartmentId)
            .OrderBy(m => m.SentAt) // Старые сверху, новые снизу
            .Take(50) // Берем последние 50 для оптимизации
            .Select(m => new ChatMessageDto(
                m.Id, 
                m.SenderId, 
                m.Sender != null ? m.Sender.Name : "Неизвестный", 
                m.Text, 
                m.SentAt))
            .ToListAsync(cancellationToken);

        return Result<List<ChatMessageDto>>.Success(messages);
    }
}