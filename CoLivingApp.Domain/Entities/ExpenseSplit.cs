namespace CoLivingApp.Domain.Entities;

public class ExpenseSplit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExpenseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; } // Доля конкретного юзера

    public Expense? Expense { get; set; }
    public User? User { get; set; }
}