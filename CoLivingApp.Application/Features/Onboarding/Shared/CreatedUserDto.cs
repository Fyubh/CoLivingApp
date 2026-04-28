namespace CoLivingApp.Application.Features.Onboarding.Shared;

/// <summary>
/// Результат успешного создания аккаунта (жильца или сотрудника).
///
/// ВАЖНО: TempPassword возвращается в plaintext ОДИН РАЗ при создании.
/// После того как админ закрыл модал — пароль больше нигде не хранится в plain
/// (в БД лежит только BCrypt-хэш). Если админ потерял пароль до того как жилец
/// сменил его — придётся сбросить заново через отдельный endpoint (будущая задача).
/// </summary>
public record CreatedUserDto(
    string UserId,
    string Email,
    string Name,
    string TempPassword
);