using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Chat.Queries;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

public record SendMessageCommand(Guid ApartmentId, string SenderId, string Text) : IRequest<Result<ChatMessageDto>>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    public SendMessageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<ChatMessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new ChatMessage
        {
            ApartmentId = request.ApartmentId,
            SenderId = request.SenderId,
            Text = request.Text
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        // Получаем имя отправителя для DTO
        var senderName = await _context.Users
            .Where(u => u.Id == request.SenderId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new ChatMessageDto(message.Id, message.SenderId, senderName ?? "Неизвестный", message.Text, message.SentAt);
        return Result<ChatMessageDto>.Success(dto);
    }
}