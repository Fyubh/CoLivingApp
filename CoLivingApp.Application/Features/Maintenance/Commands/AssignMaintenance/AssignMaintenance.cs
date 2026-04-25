using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.AssignMaintenance;

/// <summary>
/// Админ назначает подрядчика на заявку.
/// Допустимые переходы:
///   Reported / Acknowledged → Assigned (первое назначение)
///   Assigned → Assigned (переназначение на другого подрядчика)
///   InProgress → НЕ допустимо (подрядчик уже начал, нужно сначала отменить у него)
/// </summary>
public record AssignMaintenanceCommand(
    string UserId,                    // админ, выполняющий назначение
    Guid MaintenanceId,
    Guid StaffAssignmentId            // на какое назначение ставим задачу
) : IRequest<Result<MaintenanceActionResult>>;

public class AssignMaintenanceCommandHandler
    : IRequestHandler<AssignMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public AssignMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        AssignMaintenanceCommand request, CancellationToken ct)
    {
        var maintenance = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);
        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.Status is not (MaintenanceStatus.Reported
                                        or MaintenanceStatus.Acknowledged
                                        or MaintenanceStatus.Assigned))
            return Result<MaintenanceActionResult>.Failure(
                $"Невозможно назначить подрядчика на заявку в статусе {maintenance.Status}.");

        // Авторизация: админ здания
        var isAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.UserId
            && s.BuildingId == maintenance.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);
        if (!isAdmin)
            return Result<MaintenanceActionResult>.Failure(
                "У вас нет прав администратора в этом здании.");

        // Проверка назначения подрядчика: существует, в этом же здании, активен,
        // имеет роль Contractor или Cleaner.
        var staff = await _context.StaffAssignments
            .FirstOrDefaultAsync(s => s.Id == request.StaffAssignmentId, ct);
        if (staff == null)
            return Result<MaintenanceActionResult>.Failure("Подрядчик не найден.");

        if (staff.BuildingId != maintenance.BuildingId)
            return Result<MaintenanceActionResult>.Failure(
                "Подрядчик работает в другом здании.");

        if (!staff.IsActive)
            return Result<MaintenanceActionResult>.Failure("Подрядчик неактивен.");

        if (staff.Role is not (StaffRole.Contractor or StaffRole.Cleaner))
            return Result<MaintenanceActionResult>.Failure(
                "Задачи можно назначать только подрядчикам или клинерам.");

        // Применяем
        maintenance.AssignedStaffAssignmentId = staff.Id;
        maintenance.Status = MaintenanceStatus.Assigned;
        maintenance.AssignedAt = DateTime.UtcNow;
        // Если админ назначает напрямую из Reported — авто-Acknowledge.
        if (maintenance.AcknowledgedAt == null)
            maintenance.AcknowledgedAt = DateTime.UtcNow;
        maintenance.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(ct);

        return Result<MaintenanceActionResult>.Success(new MaintenanceActionResult(
            maintenance.Id,
            maintenance.BuildingId,
            maintenance.ReportedByUserId,
            AssignedStaffUserId: staff.UserId,
            maintenance.Status));
    }
}