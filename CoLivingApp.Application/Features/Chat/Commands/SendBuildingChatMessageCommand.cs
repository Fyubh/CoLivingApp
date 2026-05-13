using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Chat.Queries;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Commands;

/// <summary>
/// Отправка сообщения в чат здания. Право писать — только у активного резидента
/// этого здания. BuildingAdmin в 8.1 не пишет от своего лица (модерация и пост
/// «от лица здания» — отдельная история фазы 8.2).
/// </summary>
public record SendBuildingChatMessageCommand(Guid BuildingId, string SenderId, string Text) : IRequest<Result<ChatMessageDto>>;

public class SendBuildingChatMessageCommandHandler : IRequestHandler<SendBuildingChatMessageCommand, Result<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    public SendBuildingChatMessageCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<ChatMessageDto>> Handle(SendBuildingChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return Result<ChatMessageDto>.Failure("Сообщение не может быть пустым.");

        var trimmed = request.Text.Trim();
        if (trimmed.Length > 2000)
            return Result<ChatMessageDto>.Failure("Сообщение слишком длинное.");

        var isResident = await _context.ApartmentMembers
            .Where(m => m.UserId == request.SenderId && m.IsActive)
            .Join(_context.Apartments, m => m.ApartmentId, a => a.Id, (m, a) => a.BuildingId)
            .AnyAsync(b => b == request.BuildingId, cancellationToken);

        if (!isResident)
            return Result<ChatMessageDto>.Failure("Нет доступа к этому чату.");

        var message = new ChatMessage
        {
            Scope = ChatScope.Building,
            BuildingId = request.BuildingId,
            SenderId = request.SenderId,
            Text = trimmed
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        var senderName = await _context.Users
            .Where(u => u.Id == request.SenderId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new ChatMessageDto(message.Id, message.SenderId, senderName ?? "Неизвестный", message.Text, message.SentAt, false);
        return Result<ChatMessageDto>.Success(dto);
    }
}
