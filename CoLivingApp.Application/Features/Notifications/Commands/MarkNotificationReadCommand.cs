using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(string UserId, Guid NotificationId)
    : IRequest<Result<Guid>>;

public class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public MarkNotificationReadCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _context.ResidentNotifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, ct);

        if (notification == null)
            return Result<Guid>.Failure("Уведомление не найдено.");

        if (notification.UserId != request.UserId)
            return Result<Guid>.Failure("Нет доступа к этому уведомлению.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedById = request.UserId;
            await _context.SaveChangesAsync(ct);
        }

        return Result<Guid>.Success(notification.Id);
    }
}
