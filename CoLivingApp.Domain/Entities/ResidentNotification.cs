using CoLivingApp.Domain.Common;
using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class ResidentNotification : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public NotificationAudience Audience { get; set; } = NotificationAudience.Personal;

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsImportant { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
