using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Inventory.Commands.MoveToCart;

// Мы используем Unit, так как нам не нужно возвращать ID, просто статус успеха
public record MoveItemToCartCommand(Guid ItemId, Guid ApartmentId, string UserId) : IRequest<Result<Unit>>;