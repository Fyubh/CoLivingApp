// Файл: CoLivingApp.Application/Features/Expenses/Queries/GetBalance/UserBalanceDto.cs
namespace CoLivingApp.Application.Features.Expenses.Queries.GetBalance;

public record UserBalanceDto(
    string UserId,
    string UserName,
    decimal Balance // Положительный — ему должны, отрицательный — он должен
);