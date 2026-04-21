using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Maintenance.Commands.CreateMaintenanceRequest;

/// <summary>
/// Команда создания maintenance-заявки жильцом.
/// 
/// Правила определения места:
/// - RoomId задан → заявка по приватной комнате, BuildingId берётся из Room → Apartment → Building.
/// - ApartmentId задан (без RoomId) → общая зона квартиры.
/// - Ни то, ни другое → общая зона здания, BuildingId передаётся явно.
/// 
/// ReportedByUserId подставляется контроллером из JWT — клиент передаёт пустой string.
/// </summary>
public record CreateMaintenanceRequestCommand(
    string ReportedByUserId,
    MaintenanceCategory Category,
    string Title,
    string Description,
    MaintenancePriority Priority = MaintenancePriority.Normal,
    Guid? RoomId = null,
    Guid? ApartmentId = null,
    Guid? BuildingId = null,
    string? PhotoUrl = null
) : IRequest<Result<Guid>>;