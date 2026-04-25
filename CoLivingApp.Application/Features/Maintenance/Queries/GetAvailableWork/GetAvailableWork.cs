using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Maintenance.Queries.GetAvailableWork;

/// <summary>
/// "Что я могу взять?" — свободные задачи в зданиях, где подрядчик назначен.
/// Логика:
/// 1. Находим все активные StaffAssignments юзера (зданий может быть несколько).
/// 2. Отбираем заявки:
///    - в этих зданиях,
///    - без назначенного подрядчика (AssignedStaffAssignmentId == null),
///    - в статусе Reported или Acknowledged.
/// 3. Если у StaffAssignment задана Specialization — фильтруем по соответствующей Category.
///    Для Cleaner без Specialization — показываем только Cleaning-категорию.
/// 4. Сортируем по приоритету (Urgent → Low), потом по дате.
/// </summary>
public record GetAvailableWorkQuery(string UserId)
    : IRequest<Result<List<WorkItemDto>>>;

public class GetAvailableWorkQueryHandler
    : IRequestHandler<GetAvailableWorkQuery, Result<List<WorkItemDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetAvailableWorkQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<WorkItemDto>>> Handle(
        GetAvailableWorkQuery request, CancellationToken ct)
    {
        // 1. Активные assignments юзера с ролями Contractor/Cleaner.
        var assignments = await _context.StaffAssignments
            .Where(s => s.UserId == request.UserId
                        && s.IsActive
                        && (s.Role == StaffRole.Contractor || s.Role == StaffRole.Cleaner))
            .Select(s => new { s.BuildingId, s.Role, s.Specialization })
            .ToListAsync(ct);

        if (assignments.Count == 0)
            return Result<List<WorkItemDto>>.Success(new List<WorkItemDto>());

        // 2. Собираем допустимые пары (BuildingId, набор Categories). Для цикла по зданиям —
        //    грузим данные из БД одним запросом по BuildingIds, потом фильтруем в памяти.
        var buildingIds = assignments.Select(a => a.BuildingId).Distinct().ToList();

        var openRequestsQuery =
            from m in _context.MaintenanceRequests
            where buildingIds.Contains(m.BuildingId)
                  && m.AssignedStaffAssignmentId == null
                  && (m.Status == MaintenanceStatus.Reported
                      || m.Status == MaintenanceStatus.Acknowledged)
            join b in _context.Buildings on m.BuildingId equals b.Id
            join apt in _context.Apartments on m.ApartmentId equals apt.Id into aptGroup
            from apt in aptGroup.DefaultIfEmpty()
            join room in _context.Rooms on m.RoomId equals room.Id into roomGroup
            from room in roomGroup.DefaultIfEmpty()
            select new
            {
                m.Id,
                m.BuildingId,
                m.Category,
                m.Priority,
                m.Status,
                m.Title,
                m.Description,
                m.PhotoUrl,
                m.CreatedAt,
                m.AssignedAt,
                m.StartedAt,
                BuildingName = b.Name,
                BuildingAddressLine = b.AddressLine,
                UnitNumber = apt != null ? apt.UnitNumber : null,
                RoomNumber = room != null ? room.Number : null
            };

        var openRequests = await openRequestsQuery.ToListAsync(ct);

        // 3. Фильтрация по специализации в памяти — правила per-building, и логика
        //    "Cleaner без Specialization видит только Cleaning" понятнее в C#.
        var result = new List<WorkItemDto>();
        foreach (var req in openRequests)
        {
            // Смотрим, есть ли у юзера подходящий assignment в этом здании:
            var matchingAssignment = assignments
                .Where(a => a.BuildingId == req.BuildingId)
                .FirstOrDefault(a => IsCategoryAllowed(a.Role, a.Specialization, req.Category));

            if (matchingAssignment == null) continue;

            result.Add(new WorkItemDto(
                req.Id, req.Title, req.Description, req.Category, req.Priority, req.Status,
                req.PhotoUrl,
                req.BuildingName, req.BuildingAddressLine, req.UnitNumber, req.RoomNumber,
                req.CreatedAt, req.AssignedAt, req.StartedAt));
        }

        // 4. Сортировка: сначала по приоритету (по убыванию), потом по дате (старые сверху).
        result = result
            .OrderByDescending(w => (int)w.Priority)
            .ThenBy(w => w.CreatedAt)
            .ToList();

        return Result<List<WorkItemDto>>.Success(result);
    }

    /// <summary>
    /// Матч: подходит ли специализация роли подрядчика под категорию заявки.
    /// Cleaner → только Cleaning.
    /// Contractor без Specialization → любая категория (универсал).
    /// Contractor со Specialization → только соответствующая категория.
    /// </summary>
    private static bool IsCategoryAllowed(StaffRole role, ContractorType? spec, MaintenanceCategory cat)
    {
        if (role == StaffRole.Cleaner)
            return cat == MaintenanceCategory.Cleaning;

        if (role == StaffRole.Contractor)
        {
            if (spec == null) return true; // универсал

            return spec switch
            {
                ContractorType.Plumbing => cat == MaintenanceCategory.Plumbing,
                ContractorType.Electric => cat == MaintenanceCategory.Electric,
                ContractorType.Hvac => cat == MaintenanceCategory.Hvac,
                ContractorType.Furniture => cat == MaintenanceCategory.Furniture,
                ContractorType.WindowsAndDoors => cat == MaintenanceCategory.WindowsAndDoors,
                ContractorType.Appliance => cat == MaintenanceCategory.Appliance,
                ContractorType.Cleaning => cat == MaintenanceCategory.Cleaning,
                ContractorType.GeneralMaintenance => true,
                _ => false
            };
        }

        return false;
    }
}