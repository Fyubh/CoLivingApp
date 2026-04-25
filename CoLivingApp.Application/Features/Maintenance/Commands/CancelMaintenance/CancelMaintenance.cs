using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.CancelMaintenance;

/// <summary>
/// Жилец отменяет свою заявку. Разрешено только до начала работы подрядчика
/// (статусы Reported, Acknowledged, Assigned). После InProgress отмена запрещена —
/// там уже проделана работа, и закрывает её подрядчик через Complete или админ через Reject.
/// </summary>
public record CancelMaintenanceCommand(
    string UserId,
    Guid MaintenanceId,
    string? Reason
) : IRequest<Result<MaintenanceActionResult>>;

public class CancelMaintenanceCommandHandler
    : IRequestHandler<CancelMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public CancelMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        CancelMaintenanceCommand request, CancellationToken ct)
    {
        var maintenance = await _context.MaintenanceRequests
            .Include(m => m.AssignedStaffAssignment)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);

        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.ReportedByUserId != request.UserId)
            return Result<MaintenanceActionResult>.Failure(
                "Отменить заявку может только её автор.");

        if (maintenance.Status is not (MaintenanceStatus.Reported
                                        or MaintenanceStatus.Acknowledged
                                        or MaintenanceStatus.Assigned))
            return Result<MaintenanceActionResult>.Failure(
                $"Невозможно отменить заявку в статусе {maintenance.Status}.");

        maintenance.Status = MaintenanceStatus.Cancelled;
        maintenance.UpdatedById = request.UserId;

        // Причину сохраняем в CompletionNotes с префиксом, чтобы не плодить новое поле.
        // (В проде стоит выделить отдельное поле CancellationReason.)
        if (!string.IsNullOrWhiteSpace(request.Reason))
            maintenance.CompletionNotes = $"[Отменено жильцом] {request.Reason.Trim()}";

        await _context.SaveChangesAsync(ct);

        return Result<MaintenanceActionResult>.Success(new MaintenanceActionResult(
            maintenance.Id,
            maintenance.BuildingId,
            maintenance.ReportedByUserId,
            AssignedStaffUserId: maintenance.AssignedStaffAssignment?.UserId,
            maintenance.Status));
    }
}