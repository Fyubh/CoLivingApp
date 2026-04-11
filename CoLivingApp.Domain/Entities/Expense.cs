namespace CoLivingApp.Domain.Entities;

using CoLivingApp.Domain.Enums;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string PayerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; } = ExpenseCategory.Other; // НОВОЕ ПОЛЕ
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public Apartment? Apartment { get; set; }
    public User? Payer { get; set; }
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}