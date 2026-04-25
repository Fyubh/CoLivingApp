using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.AcknowledgeMaintenance;

/// <summary>
/// Админ здания подтверждает заявку. Переход Reported → Acknowledged.
/// Назначение подрядчика — отдельная команда (Assign).
/// </summary>
public record AcknowledgeMaintenanceCommand(
    string UserId,
    Guid MaintenanceId
) : IRequest<Result<MaintenanceActionResult>>;

public class AcknowledgeMaintenanceCommandHandler
    : IRequestHandler<AcknowledgeMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public AcknowledgeMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        AcknowledgeMaintenanceCommand request, CancellationToken ct)
    {
        var maintenance = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);
        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.Status != MaintenanceStatus.Reported)
            return Result<MaintenanceActionResult>.Failure(
                $"Невозможно подтвердить заявку в статусе {maintenance.Status}.");

        // Авторизация: юзер должен быть активным BuildingAdmin этого здания.
        var isBuildingAdmin = await _context.StaffAssignments.AnyAsync(s =>
            s.UserId == request.UserId
            && s.BuildingId == maintenance.BuildingId
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive, ct);

        if (!isBuildingAdmin)
            return Result<MaintenanceActionResult>.Failure(
                "У вас нет прав администратора в этом здании.");

        maintenance.Status = MaintenanceStatus.Acknowledged;
        maintenance.AcknowledgedAt = DateTime.UtcNow;
        maintenance.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(ct);

        return Result<MaintenanceActionResult>.Success(new MaintenanceActionResult(
            maintenance.Id,
            maintenance.BuildingId,
            maintenance.ReportedByUserId,
            AssignedStaffUserId: null,
            maintenance.Status));
    }
}