using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Application.Features.Notifications.Shared;

public record ResidentNotificationDto(
    Guid Id,
    Guid BuildingId,
    NotificationAudience Audience,
    string Title,
    string Body,
    bool IsImportant,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);
