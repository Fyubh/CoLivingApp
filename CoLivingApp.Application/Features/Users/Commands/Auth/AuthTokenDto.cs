namespace CoLivingApp.Application.Features.Users.Commands.Auth;

/// <summary>
/// Общий response для Login и ChangePassword.
/// 
/// Зачем общий?
/// - Login возвращает JWT и флаг, нужно ли менять пароль.
/// - ChangePassword возвращает НОВЫЙ JWT (с обновлённым набором claim'ов:
///   уже без mustChangePassword) — фронт сразу его сохраняет и продолжает работу,
///   не отправляя юзера на форму логина повторно.
/// </summary>
public record AuthTokenDto(
    string Token,
    bool MustChangePassword
);