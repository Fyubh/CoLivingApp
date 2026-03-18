// Файл: CoLivingApp.Application/Features/Apartments/Commands/JoinApartment/JoinApartmentCommand.cs
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Apartments.Commands.JoinApartment;

/// <summary>
/// Команда для вступления в существующую квартиру по коду приглашения.
/// </summary>
public record JoinApartmentCommand(
    string InviteCode, 
    string UserId
) : IRequest<Result<Guid>>;