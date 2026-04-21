using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetMyMaintenanceRequests;

/// <summary>
/// DTO для отображения maintenance-заявки жильцу.
/// Без внутренних ID'ов StaffAssignment — жилец видит только имя подрядчика.
/// </summary>
public record MaintenanceRequestDto(
    Guid Id,
    string Title,
    string Description,
    MaintenanceCategory Category,
    MaintenancePriority Priority,
    MaintenanceStatus Status,
    string? PhotoUrl,
    string? CompletionPhotoUrl,
    string? CompletionNotes,
    string? AssignedStaffName,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int? ResidentRating
);