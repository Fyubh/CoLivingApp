using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Chat.Queries;

/// <summary>
/// История чата здания. Доступ: активный резидент здания (есть ApartmentMember
/// в любой квартире этого здания) ИЛИ активный BuildingAdmin этого здания
/// (для read-only viewer в админ-панели).
/// </summary>
public record GetBuildingChatHistoryQuery(Guid BuildingId, string RequestingUserId) : IRequest<Result<List<ChatMessageDto>>>;

public class GetBuildingChatHistoryQueryHandler : IRequestHandler<GetBuildingChatHistoryQuery, Result<List<ChatMessageDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetBuildingChatHistoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ChatMessageDto>>> Handle(GetBuildingChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var hasAccess = await UserCanReadBuildingChat(request.BuildingId, request.RequestingUserId, cancellationToken);
        if (!hasAccess)
            return Result<List<ChatMessageDto>>.Failure("Нет доступа к этому чату.");

        var blockedIds = await _context.ChatBlocks
            .Where(b => b.BlockerId == request.RequestingUserId)
            .Select(b => b.BlockedUserId)
            .ToListAsync(cancellationToken);

        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.Scope == ChatScope.Building && m.BuildingId == request.BuildingId)
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

    private async Task<bool> UserCanReadBuildingChat(Guid buildingId, string userId, CancellationToken ct)
    {
        var isResident = await _context.ApartmentMembers
            .Where(m => m.UserId == userId && m.IsActive)
            .Join(_context.Apartments, m => m.ApartmentId, a => a.Id, (m, a) => a.BuildingId)
            .AnyAsync(b => b == buildingId, ct);

        if (isResident) return true;

        return await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == userId
            && s.BuildingId == buildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
    }
}
