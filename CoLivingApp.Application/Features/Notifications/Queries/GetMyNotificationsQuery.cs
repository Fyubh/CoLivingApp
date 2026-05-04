using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Notifications.Shared;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(string UserId, bool UnreadOnly = false, int Take = 50)
    : IRequest<Result<List<ResidentNotificationDto>>>;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, Result<List<ResidentNotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetMyNotificationsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<ResidentNotificationDto>>> Handle(
        GetMyNotificationsQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return Result<List<ResidentNotificationDto>>.Failure("Не указан пользователь.");

        var take = Math.Clamp(request.Take, 1, 100);
        var query = _context.ResidentNotifications
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var items = await query
            .OrderByDescending(n => n.IsImportant)
            .ThenByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new ResidentNotificationDto(
                n.Id,
                n.BuildingId,
                n.Audience,
                n.Title,
                n.Body,
                n.IsImportant,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt))
            .ToListAsync(ct);

        return Result<List<ResidentNotificationDto>>.Success(items);
    }
}
