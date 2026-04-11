using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class Chore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedUserId { get; set; } 
    
    // Новые поля:
    public ChoreStatus Status { get; set; } = ChoreStatus.Pending;
    public DateTime? DueDate { get; set; } // Дедлайн (может быть без дедлайна)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Apartment? Apartment { get; set; }
    public User? AssignedUser { get; set; }
}