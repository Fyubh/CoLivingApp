using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Admin.Queries.GetTenants;

// Запрос для получения списка жильцов. Можно передать фильтры.
public class GetTenantsQuery : IRequest<Result<List<TenantDto>>>
{
    public string? SearchTerm { get; set; } // Для поиска по имени/email
    public bool SortByKarmaAscending { get; set; } // Чтобы быстро найти нарушителей
}

// DTO (Data Transfer Object) - отдаем клиенту (админ-панели) только то, что нужно
public class TenantDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int KarmaScore { get; set; }
    // В будущем сюда добавим номер комнаты и статус контракта
}