namespace CoLivingApp.Domain.Entities;

public class Apartment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApartmentMember> Members { get; set; } = new List<ApartmentMember>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}