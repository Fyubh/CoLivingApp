using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Notifications.Commands;

public record SendBuildingNotificationCommand(
    string AdminUserId,
    Guid BuildingId,
    string Title,
    string Body,
    bool IsImportant
) : IRequest<Result<int>>;

public class SendBuildingNotificationCommandHandler
    : IRequestHandler<SendBuildingNotificationCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    public SendBuildingNotificationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<int>> Handle(SendBuildingNotificationCommand request, CancellationToken ct)
    {
        var validation = ValidateText(request.Title, request.Body);
        if (validation != null) return Result<int>.Failure(validation);

        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.AdminUserId
            && s.BuildingId == request.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);

        if (!isAdmin)
            return Result<int>.Failure("У вас нет прав администратора в этом здании.");

        var residentIds = await _context.ApartmentMembers
            .Where(m => m.IsActive && m.Apartment!.BuildingId == request.BuildingId)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (residentIds.Count == 0)
            return Result<int>.Failure("В здании нет активных жильцов.");

        var title = request.Title.Trim();
        var body = request.Body.Trim();

        var notifications = residentIds.Select(userId => new ResidentNotification
        {
            BuildingId = request.BuildingId,
            UserId = userId,
            Audience = NotificationAudience.Building,
            Title = title,
            Body = body,
            IsImportant = request.IsImportant,
            CreatedById = request.AdminUserId
        }).ToList();

        _context.ResidentNotifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(notifications.Count);
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
