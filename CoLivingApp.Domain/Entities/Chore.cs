// Добавь using
using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class Chore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; } 
    public ChoreCategory Category { get; set; } = ChoreCategory.Other;
    
    public string? AssignedUserId { get; set; } 
    public ChoreStatus Status { get; set; } = ChoreStatus.Pending;
    public DateTime? DueDate { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Apartment? Apartment { get; set; }
    public User? AssignedUser { get; set; }
}