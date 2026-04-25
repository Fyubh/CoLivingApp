using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetAvailableWork;

/// <summary>
/// DTO задачи для подрядчика. Используется и в GetAvailableWork, и в GetMyAssignedWork.
/// Включает название здания — подрядчик может работать в нескольких и ему нужно понимать, куда ехать.
/// </summary>
public record WorkItemDto(
    Guid Id,
    string Title,
    string Description,
    MaintenanceCategory Category,
    MaintenancePriority Priority,
    MaintenanceStatus Status,
    string? PhotoUrl,

    // Локация
    string BuildingName,
    string BuildingAddressLine,
    string? UnitNumber,
    string? RoomNumber,

    // Тайминг
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? StartedAt
);