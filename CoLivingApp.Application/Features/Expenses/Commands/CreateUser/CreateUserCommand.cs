// Файл: CoLivingApp.Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Команда на создание пользователя (вызывается после успешной регистрации на мобилке).
/// Возвращает Id созданного пользователя (строку).
/// </summary>
public record CreateUserCommand(
    string Id,       // ID от внешнего провайдера (Firebase / ASP.NET Identity)
    string Email, 
    string Name
) : IRequest<Result<string>>;