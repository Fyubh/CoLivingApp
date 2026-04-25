using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetBuildingMaintenanceRequests;

/// <summary>
/// DTO для админской таблицы заявок по зданию. Богаче чем тот, что видит жилец:
/// включает имя автора заявки + unit-номер для быстрой ориентации на дашборде.
/// </summary>
public record BuildingMaintenanceRequestDto(
    Guid Id,
    string Title,
    string Description,
    MaintenanceCategory Category,
    MaintenancePriority Priority,
    MaintenanceStatus Status,
    string ReporterName,
    string? UnitNumber,
    string? RoomNumber,
    string? PhotoUrl,
    string? AssignedStaffName,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int? ResidentRating
);