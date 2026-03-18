// Файл: CoLivingApp.Application/Features/Shopping/Commands/Checkout/CheckoutCommand.cs
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Shopping.Commands.Checkout;

/// <summary>
/// Команда для оплаты выбранных товаров из корзины.
/// Она обновляет статус товаров и автоматически создает общий чек (Expense).
/// </summary>
public record CheckoutCommand(
    Guid ApartmentId,
    string PayerId,
    decimal TotalAmount,
    List<Guid> ItemIds // ID товаров, которые мы выбрали галочками для покупки
) : IRequest<Result<Guid>>;