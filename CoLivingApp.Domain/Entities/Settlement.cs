namespace CoLivingApp.Domain.Entities;

public class Settlement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public Apartment? Apartment { get; set; }
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
}