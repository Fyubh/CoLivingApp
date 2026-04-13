using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class RecurringChore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedUserId { get; set; } // Кому назначено
    
    public RecurrencePattern Pattern { get; set; }
    public int Interval { get; set; } = 1;
    public DateTime NextRunDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Apartment? Apartment { get; set; }
    public User? AssignedUser { get; set; }
    public string? Description { get; set; } 
    public ChoreCategory Category { get; set; } = ChoreCategory.Other;
}