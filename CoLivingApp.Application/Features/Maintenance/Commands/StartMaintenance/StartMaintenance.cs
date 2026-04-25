using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.StartMaintenance;

/// <summary>
/// Подрядчик берёт задачу в работу. Assigned → InProgress.
/// Только тот подрядчик, которому задача назначена, может её начать.
/// </summary>
public record StartMaintenanceCommand(
    string UserId,
    Guid MaintenanceId
) : IRequest<Result<MaintenanceActionResult>>;

public class StartMaintenanceCommandHandler
    : IRequestHandler<StartMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public StartMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        StartMaintenanceCommand request, CancellationToken ct)
    {
        // Вытаскиваем заявку вместе со связанным StaffAssignment, чтобы проверить UserId.
        var maintenance = await _context.MaintenanceRequests
            .Include(m => m.AssignedStaffAssignment)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);

        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.Status != MaintenanceStatus.Assigned)
            return Result<MaintenanceActionResult>.Failure(
                $"Невозможно начать заявку в статусе {maintenance.Status}.");

        if (maintenance.AssignedStaffAssignment == null
            || maintenance.AssignedStaffAssignment.UserId != request.UserId)
            return Result<MaintenanceActionResult>.Failure(
                "Заявка назначена другому подрядчику.");

        maintenance.Status = MaintenanceStatus.InProgress;
        maintenance.StartedAt = DateTime.UtcNow;
        maintenance.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(ct);

        return Result<MaintenanceActionResult>.Success(new MaintenanceActionResult(
            maintenance.Id,
            maintenance.BuildingId,
            maintenance.ReportedByUserId,
            AssignedStaffUserId: maintenance.AssignedStaffAssignment.UserId,
            maintenance.Status));
    }
}