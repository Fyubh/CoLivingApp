using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Users.Commands.Auth;

public record RegisterCommand(string Email, string Name, string Password) : IRequest<Result<string>>;