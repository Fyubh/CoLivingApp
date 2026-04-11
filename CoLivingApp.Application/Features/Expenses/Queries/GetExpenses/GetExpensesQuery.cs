using CoLivingApp.Domain.Shared;
using MediatR;

namespace CoLivingApp.Application.Features.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(Guid ApartmentId) : IRequest<Result<List<ExpenseDto>>>;

// В DTO добавлено поле CategoryId
public record ExpenseDto(
    Guid Id,
    string Description,
    decimal Amount,
    DateTime Date,
    string PayerId,
    string PayerName,
    int CategoryId 
);