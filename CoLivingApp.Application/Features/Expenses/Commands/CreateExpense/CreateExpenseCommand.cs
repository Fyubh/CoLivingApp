// Файл: CoLivingApp.Application/Features/Expenses/Commands/CreateExpense/CreateExpenseCommand.cs
using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Expenses.Commands.CreateExpense;

/// <summary>
/// Команда на добавление общего расхода.
/// </summary>
public record CreateExpenseCommand(
    Guid ApartmentId,
    string PayerId,
    decimal Amount,
    string Description
) : IRequest<Result<Guid>>;