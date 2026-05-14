using System.Security.Claims;
using CoLivingApp.Infrastructure.Persistence;
using CoLivingApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace CoLivingApp.Api.Hubs;

/// <summary>
/// Хаб SignalR — точка подключения для real-time уведомлений.
/// Клиент подписывается на одну или несколько "групп" в зависимости от своих ролей:
/// - Apartment group — обновления roommate-функций (чат, траты, инвентарь).
/// - Building group — чат всего здания (community-layer).
/// - User group — персональные пуши (моя заявка сменила статус / мне назначили задачу).
/// - Building admin group — админ здания получает новые заявки.
///
/// Префиксы:
/// - Apartment: БЕЗ префикса — исторически сложилось, ChatController/ExpensesController/
///   InventoryController шлют в Group(apartmentId.ToString()); менять нельзя без их правки.
/// - Building: "building_{buildingId}".
/// - User: "user_{userId}".
/// - Building admin: "building_admin_{buildingId}".
/// </summary>
[Authorize]
public class CoLivingHub : Hub
{
    private readonly ApplicationDbContext _db;

    public CoLivingHub(ApplicationDbContext db) => _db = db;

    /// <summary>Подписка на обновления квартиры — roommate-слой.</summary>
    public async Task JoinApartmentGroup(string apartmentId)
    {
        var userId = CurrentUserId();
        if (userId == null || !Guid.TryParse(apartmentId, out var apartmentGuid))
            throw new HubException("Unauthorized group subscription.");

        var isMember = await _db.ApartmentMembers.AnyAsync(m =>
            m.UserId == userId && m.ApartmentId == apartmentGuid && m.IsActive);

        if (!isMember)
            throw new HubException("Unauthorized group subscription.");

        await Groups.AddToGroupAsync(Context.ConnectionId, apartmentGuid.ToString());
    }

    /// <summary>
    /// Подписка на чат здания — community-слой. Право подписки —
    /// у активного резидента (есть ApartmentMember в любой квартире этого
    /// здания) ИЛИ у активного BuildingAdmin (для read-only viewer).
    /// </summary>
    public async Task JoinBuildingGroup(string buildingId)
    {
        var userId = CurrentUserId();
        if (userId == null || !Guid.TryParse(buildingId, out var buildingGuid))
            throw new HubException("Unauthorized group subscription.");

        var isResident = await _db.ApartmentMembers
            .Where(m => m.UserId == userId && m.IsActive)
            .Join(_db.Apartments, m => m.ApartmentId, a => a.Id, (m, a) => a.BuildingId)
            .AnyAsync(b => b == buildingGuid);

        if (!isResident)
        {
            var isAdmin = await _db.StaffAssignments.AnyAsync(s =>
                s.UserId == userId
                && s.BuildingId == buildingGuid
                && s.Role == StaffRole.BuildingAdmin
                && s.IsActive);

            if (!isAdmin)
                throw new HubException("Unauthorized group subscription.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"building_{buildingGuid}");
    }

    /// <summary>
    /// Подписка на персональные пуши. Клиент вызывает сразу после соединения.
    /// </summary>
    public async Task JoinUserGroup(string userId)
    {
        if (CurrentUserId() != userId)
            throw new HubException("Unauthorized group subscription.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    /// <summary>
    /// Подписка на пуши по зданию для админов. Вызывается по одному разу
    /// на каждое здание, в котором юзер — BuildingAdmin.
    /// </summary>
    public async Task JoinBuildingAdminGroup(string buildingId)
    {
        var userId = CurrentUserId();
        if (userId == null || !Guid.TryParse(buildingId, out var buildingGuid))
            throw new HubException("Unauthorized group subscription.");

        var isAdmin = await _db.StaffAssignments.AnyAsync(s =>
            s.UserId == userId
            && s.BuildingId == buildingGuid
            && s.Role == StaffRole.BuildingAdmin
            && s.IsActive);

        if (!isAdmin)
            throw new HubException("Unauthorized group subscription.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"building_admin_{buildingGuid}");
    }

    private string? CurrentUserId() => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
