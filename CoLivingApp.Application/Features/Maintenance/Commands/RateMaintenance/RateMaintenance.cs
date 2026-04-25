using CoLivingApp.Application.Abstractions;
using CoLivingApp.Application.Features.Maintenance.Shared;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Commands.RateMaintenance;

/// <summary>
/// Жилец ставит оценку после закрытия заявки. Допустимо только в статусе Completed,
/// и только одна оценка на заявку (повторно менять нельзя — чтобы защитить историю метрик).
/// 
/// Побочный эффект: пересчитываем AverageRating подрядчика как среднее всех ResidentRating
/// по его закрытым задачам. Делаем через подзапрос в БД, а не в памяти —
/// у старшего подрядчика могут быть тысячи оценок.
/// </summary>
public record RateMaintenanceCommand(
    string UserId,
    Guid MaintenanceId,
    int Rating,
    string? Feedback
) : IRequest<Result<MaintenanceActionResult>>;

public class RateMaintenanceCommandHandler
    : IRequestHandler<RateMaintenanceCommand, Result<MaintenanceActionResult>>
{
    private readonly IApplicationDbContext _context;
    public RateMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MaintenanceActionResult>> Handle(
        RateMaintenanceCommand request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5)
            return Result<MaintenanceActionResult>.Failure("Оценка должна быть от 1 до 5.");

        var maintenance = await _context.MaintenanceRequests
            .Include(m => m.AssignedStaffAssignment)
            .FirstOrDefaultAsync(m => m.Id == request.MaintenanceId, ct);

        if (maintenance == null)
            return Result<MaintenanceActionResult>.Failure("Заявка не найдена.");

        if (maintenance.ReportedByUserId != request.UserId)
            return Result<MaintenanceActionResult>.Failure(
                "Оценить заявку может только её автор.");

        if (maintenance.Status != MaintenanceStatus.Completed)
            return Result<MaintenanceActionResult>.Failure(
                "Оценить можно только закрытую заявку.");

        if (maintenance.ResidentRating.HasValue)
            return Result<MaintenanceActionResult>.Failure(
                "Вы уже оценили эту заявку.");

        if (maintenance.AssignedStaffAssignment == null)
            return Result<MaintenanceActionResult>.Failure(
                "У заявки нет назначенного подрядчика — оценивать некого.");

        maintenance.ResidentRating = request.Rating;
        maintenance.ResidentFeedback = request.Feedback?.Trim();
        maintenance.UpdatedById = request.UserId;

        // Пересчитываем среднюю оценку подрядчика в этом здании.
        // Включаем текущую оценку (maintenance.ResidentRating уже установлен, но ещё не сохранён).
        // Поэтому берём все прошлые оценки + добавляем новую.
        var staffAssignmentId = maintenance.AssignedStaffAssignmentId!.Value;
        var ratings = await _context.MaintenanceRequests
            .Where(m => m.AssignedStaffAssignmentId == staffAssignmentId
                        && m.ResidentRating != null
                        && m.Id != maintenance.Id) // исключаем текущую — её добавим ниже
            .Select(m => m.ResidentRating!.Value)
            .ToListAsync(ct);
        ratings.Add(request.Rating);

        var average = (decimal)ratings.Average();
        maintenance.AssignedStaffAssignment.AverageRating = Math.Round(average, 2);
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