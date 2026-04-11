namespace CoLivingApp.Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Пусть генерируется автоматически
    public string Email { get; set;  } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // НОВОЕ ПОЛЕ

    public ICollection<ApartmentMember> ApartmentMembers { get; set; } = new List<ApartmentMember>();
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
}