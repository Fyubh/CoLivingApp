namespace CoLivingApp.Domain.Entities;

public class ApartmentMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ApartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    public User? User { get; set; }
    public Apartment? Apartment { get; set; }
}