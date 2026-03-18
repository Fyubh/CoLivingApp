// Файл: CoLivingApp.Application/Features/Shopping/Commands/Checkout/CheckoutCommandHandler.cs
using CoLivingApp.Application.Abstractions;
using CoLivingApp.Domain.Entities;
using CoLivingApp.Domain.Enums;
using CoLivingApp.Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoLivingApp.Application.Features.Shopping.Commands.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CheckoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Проверяем, есть ли активные жильцы для деления чека
        var members = await _context.ApartmentMembers
            .Where(m => m.ApartmentId == request.ApartmentId && m.IsActive)
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
            return Result<Guid>.Failure("В квартире нет активных жильцов для деления чека.");

        // 2. Достаем товары, которые мы покупаем
        var itemsToBuy = await _context.InventoryItems
            .Where(i => request.ItemIds.Contains(i.Id) && i.ApartmentId == request.ApartmentId)
            .ToListAsync(cancellationToken);

        if (itemsToBuy.Count != request.ItemIds.Count)
            return Result<Guid>.Failure("Некоторые товары не найдены или не принадлежат этой квартире.");

        // 3. ОБНОВЛЯЕМ ИНВЕНТАРЬ: Возвращаем товары в холодильник
        foreach (var item in itemsToBuy)
        {
            item.Status = ItemStatus.Available;
            item.PurchasedAt = DateTime.UtcNow;
            item.UpdatedById = request.PayerId;
        }

        // 4. СОЗДАЕМ ЧЕК (Expense)
        // Генерируем красивое описание, например: "Покупка: Apple, Bread..."
        var itemNames = string.Join(", ", itemsToBuy.Select(i => i.CustomName ?? "Товар"));
        var description = $"Shopping List Checkout: {itemNames}".PadRight(500).TrimEnd(); // Защита от слишком длинного текста

        var expense = new Expense
        {
            ApartmentId = request.ApartmentId,
            PayerId = request.PayerId,
            Amount = request.TotalAmount,
            Description = description,
            Date = DateTime.UtcNow
        };

        // 5. ДЕЛИМ СУММУ НА ВСЕХ (Сплиты)
        decimal splitAmount = request.TotalAmount / members.Count;
        foreach (var member in members)
        {
            expense.Splits.Add(new ExpenseSplit
            {
                ExpenseId = expense.Id,
                UserId = member.UserId,
                Amount = splitAmount
            });
        }

        _context.Expenses.Add(expense);

        // 6. Сохраняем всё вместе! Если упадет одно — отменится всё (EF Core Transaction)
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(expense.Id);
    }
}