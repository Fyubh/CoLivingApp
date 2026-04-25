using Microsoft.AspNetCore.SignalR;

namespace CoLivingApp.Api.Hubs;

/// <summary>
/// Хаб SignalR — точка подключения для real-time уведомлений.
/// Клиент подписывается на одну или несколько "групп" в зависимости от своих ролей:
/// - Apartment group — обновления roommate-функций (чат, траты, инвентарь).
/// - User group — персональные пуши (моя заявка сменила статус / мне назначили задачу).
/// - Building admin group — админ здания получает новые заявки.
/// 
/// Префиксы:
/// - Apartment: БЕЗ префикса — исторически сложилось, ChatController/ExpensesController/
///   InventoryController шлют в Group(apartmentId.ToString()); менять нельзя без их правки.
/// - User: "user_{userId}".
/// - Building admin: "building_admin_{buildingId}".
/// </summary>
public class CoLivingHub : Hub
{
    /// <summary>Подписка на обновления квартиры — roommate-слой.</summary>
    public async Task JoinApartmentGroup(string apartmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, apartmentId);
    }

    /// <summary>
    /// Подписка на персональные пуши. Клиент вызывает сразу после соединения.
    /// </summary>
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    /// <summary>
    /// Подписка на пуши по зданию для админов. Вызывается по одному разу
    /// на каждое здание, в котором юзер — BuildingAdmin.
    /// </summary>
    public async Task JoinBuildingAdminGroup(string buildingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"building_admin_{buildingId}");
    }
}