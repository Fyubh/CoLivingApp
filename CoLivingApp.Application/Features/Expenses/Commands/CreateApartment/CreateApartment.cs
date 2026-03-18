// Файл: CoLivingApp.Application/Features/Apartments/Commands/CreateApartment/CreateApartmentCommand.cs
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Apartments.Commands.CreateApartment;

/// <summary>
/// Команда на создание новой квартиры.
/// IRequest<Result<Guid>> означает, что в результате успешного выполнения 
/// мы вернем ID созданной квартиры, обернутый в наш красивый класс Result.
/// </summary>
public record CreateApartmentCommand(
    string Name, 
    string CreatorUserId
) : IRequest<Result<Guid>>;