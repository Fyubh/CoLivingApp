using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

public record LoginCommand(string Email, string Password) : IRequest<Result<string>>; // Вернет JWT токен