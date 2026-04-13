using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class RecurringExpense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string PayerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    
    // Настройки повторения
    public RecurrencePattern Pattern { get; set; }
    public int Interval { get; set; } = 1; // 1 = каждый месяц, 2 = каждые 2 месяца и тд.
    public DateTime NextRunDate { get; set; } // Когда должен создаться следующий чек (UTC)
    public bool IsActive { get; set; } = true;

    public Apartment? Apartment { get; set; }
    public User? Payer { get; set; }
}