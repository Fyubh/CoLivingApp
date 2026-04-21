using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetMyMaintenanceRequests;

/// <summary>
/// Список maintenance-заявок текущего жильца, отсортированный по дате создания (новые сверху).
/// Возвращает как открытые, так и закрытые заявки — фильтрация по статусу делается на клиенте.
/// </summary>
public record GetMyMaintenanceRequestsQuery(string UserId)
    : IRequest<Result<List<MaintenanceRequestDto>>>;