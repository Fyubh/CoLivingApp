using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Application.Features.Maintenance.Shared;

/// <summary>
/// Общий результат всех workflow-команд maintenance (Acknowledge/Assign/Start/Complete/Cancel/Rate).
/// Содержит достаточно данных, чтобы контроллер мог разослать SignalR-пуши без дополнительных запросов к БД.
/// 
/// Это сознательный компромисс чистой архитектуры:
/// - Application-слой НЕ знает про SignalR (правильно).
/// - Но handler всё равно "знает", какие ID нужны для пушей — он их и возвращает.
/// - Controller, который уже работает с IHubContext, рассылает пуши.
/// </summary>
public record MaintenanceActionResult(
    Guid MaintenanceId,
    Guid BuildingId,
    string ReporterUserId,
    string? AssignedStaffUserId,
    MaintenanceStatus NewStatus
);