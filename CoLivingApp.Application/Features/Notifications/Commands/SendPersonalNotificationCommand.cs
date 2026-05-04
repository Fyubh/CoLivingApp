using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Notifications.Commands;

public record SendPersonalNotificationCommand(
    string AdminUserId,
    Guid BuildingId,
    string TargetUserId,
    string Title,
    string Body,
    bool IsImportant
) : IRequest<Result<Guid>>;

public class SendPersonalNotificationCommandHandler
    : IRequestHandler<SendPersonalNotificationCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public SendPersonalNotificationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(SendPersonalNotificationCommand request, CancellationToken ct)
    {
        var validation = ValidateText(request.Title, request.Body);
        if (validation != null) return Result<Guid>.Failure(validation);

        var isAdmin = await IsBuildingAdmin(request.AdminUserId, request.BuildingId, ct);
        if (!isAdmin)
            return Result<Guid>.Failure("У вас нет прав администратора в этом здании.");

        var targetLivesInBuilding = await _context.ApartmentMembers.AnyAsync(m =>
            m.UserId == request.TargetUserId
            && m.IsActive
            && m.Apartment!.BuildingId == request.BuildingId, ct);

        if (!targetLivesInBuilding)
            return Result<Guid>.Failure("Жилец не найден в выбранном здании.");

        var notification = new ResidentNotification
        {
            BuildingId = request.BuildingId,
            UserId = request.TargetUserId,
            Audience = NotificationAudience.Personal,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            IsImportant = request.IsImportant,
            CreatedById = request.AdminUserId
        };

        _context.ResidentNotifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(notification.Id);
    }

    private async Task<bool> IsBuildingAdmin(string userId, Guid buildingId, CancellationToken ct)
    {
        return await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == userId
            && s.BuildingId == buildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
    }

    private static string? ValidateText(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Заголовок обязателен.";
        if (title.Trim().Length > 160) return "Заголовок должен быть не длиннее 160 символов.";
        if (string.IsNullOrWhiteSpace(body)) return "Текст уведомления обязателен.";
        if (body.Trim().Length > 2000) return "Текст уведомления должен быть не длиннее 2000 символов.";
        return null;
    }
}
