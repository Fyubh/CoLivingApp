using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Inventory.Commands.RemoveItem;

public record RemoveItemCommand(Guid ItemId, Guid ApartmentId) : IRequest<Result<Unit>>;