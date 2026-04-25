using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.CompleteMaintenance;

/// <summary>
/// Подрядчик закрывает задачу. InProgress → Completed.
/// Принимаем также Assigned → Completed (подрядчик пропустил Start), чтобы не блокировать UX.
/// 
/// CompletionPhotoUrl пока принимаем как string. В следующей сессии (Phase 4) заменим
/// на multipart-upload через IFileStorageService.
/// 
/// Побочный эффект: увеличиваем счётчик CompletedTasksCount на StaffAssignment.
/// AverageRating обновляется в RateMaintenance когда жилец поставит оценку.
/// </summary>
public record CompleteMaintenanceCommand(
    string UserId,
    Guid MaintenanceId,
    string? CompletionNotes,
    string? CompletionPhotoUrl
) : IRequest<Result<MaintenanceActionResult>>;

public class CompleteMaintenanceCommandHandler
    : IRequestHandler<CompleteMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public CompleteMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        CompleteMaintenanceCommand request, CancellationToken ct)
    {
        var maintenance = await _context.MaintenanceRequests
            .Include(m => m.AssignedStaffAssignment)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);

        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.Status is not (MaintenanceStatus.InProgress or MaintenanceStatus.Assigned))
            return Result<MaintenanceActionResult>.Failure(
                $"Невозможно закрыть заявку в статусе {maintenance.Status}.");

        if (maintenance.AssignedStaffAssignment == null
            || maintenance.AssignedStaffAssignment.UserId != request.UserId)
            return Result<MaintenanceActionResult>.Failure(
                "Заявка назначена другому подрядчику.");

        maintenance.Status = MaintenanceStatus.Completed;
        maintenance.CompletedAt = DateTime.UtcNow;
        maintenance.CompletionNotes = request.CompletionNotes?.Trim();
        maintenance.CompletionPhotoUrl = request.CompletionPhotoUrl;
        maintenance.UpdatedById = request.UserId;

        // Если подрядчик пропустил Start — выставим StartedAt, чтобы не было "дыры" в аудите.
        if (maintenance.StartedAt == null)
            maintenance.StartedAt = maintenance.CompletedAt;

        // Инкремент счётчика задач подрядчика.
        maintenance.AssignedStaffAssignment.CompletedTasksCount += 1;
        maintenance.AssignedStaffAssignment.UpdatedById = request.UserId;

        await _context.SaveChangesAsync(ct);

        return Result<MaintenanceActionResult>.Success(new MaintenanceActionResult(
            maintenance.Id,
            maintenance.BuildingId,
            maintenance.ReportedByUserId,
            AssignedStaffUserId: maintenance.AssignedStaffAssignment.UserId,
            maintenance.Status));
    }
}