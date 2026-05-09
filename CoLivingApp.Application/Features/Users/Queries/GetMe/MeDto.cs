namespace CoLivingApp.Application.Features.Users.Queries.GetMe;

/// <summary>
/// Информация о самом пользователе для экрана «Профиль».
/// Имя и роль здесь дублируют данные из JWT, но клиенту удобнее тянуть
/// одним запросом, чем парсить токен. Email на клиенте больше нигде не
/// показывается — Profile единственный потребитель.
///
/// Role сериализуется как строка ("Tenant", "Staff", "Admin", "SuperAdmin")
/// — JsonStringEnumConverter уже зарегистрирован глобально в Program.cs.
/// </summary>
public record MeDto(
    string Id,
    string Email,
    string Name,
    string Role,
    int AccessLevel);
