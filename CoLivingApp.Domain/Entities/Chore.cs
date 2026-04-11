namespace CoLivingApp.Domain.Entities;

public class Chore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedUserId { get; set; } // Если null — задача общая (кто первый, тот и сделал)
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Apartment? Apartment { get; set; }
    public User? AssignedUser { get; set; }
}