namespace CoLivingApp.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set;  } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // Навигационные свойства для EF Core
    public ICollection<ApartmentMember> ApartmentMembers { get; set; } = new List<ApartmentMember>();
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
}