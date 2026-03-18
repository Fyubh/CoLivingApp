// Файл: CoLivingApp.Application/Features/Inventory/Commands/CreateItem/CreateItemCommand.cs
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Inventory.Commands.CreateItem;

public record CreateItemCommand(
    Guid ApartmentId,
    string? CustomName,
    decimal Quantity,
    UnitType Unit,
    ItemStatus Status, // Сразу выбираем: в инвентарь или в корзину
    string UserId
) : IRequest<Result<Guid>>;