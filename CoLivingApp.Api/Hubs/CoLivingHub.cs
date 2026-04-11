using Microsoft.AspNetCore.SignalR;

namespace CoLivingApp.Api.Hubs;

// Хаб — это точка подключения для веб-сокетов
public class CoLivingHub : Hub
{
    // Метод, который фронтенд вызовет сразу после подключения
    public async Task JoinApartmentGroup(string apartmentId)
    {
        // Добавляем пользователя в "комнату" его квартиры.
        // Теперь мы сможем рассылать уведомления только жильцам этой квартиры!
        await Groups.AddToGroupAsync(Context.ConnectionId, apartmentId);
    }
}