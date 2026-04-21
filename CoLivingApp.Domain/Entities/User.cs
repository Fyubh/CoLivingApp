using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // --- НОВЫЕ ПОЛЯ ДЛЯ ADMIN PANEL ---
    
    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Tenant;

    /// <summary>
    /// Уровень доступа (от 1 до 10). 
    /// 1 - Жильцы/Базовый клининг
    /// 5 - Администратор ресепшена
    /// 10 - Генеральный менеджер
    /// </summary>
    public int AccessLevel { get; set; } = 1;

    /// <summary>
    /// Dorm Social Credit (Карма). 
    /// Начисляется за чистоту/своевременную оплату, списывается за штрафы.
    /// </summary>
    public int KarmaScore { get; set; } = 100;

    // ----------------------------------

    public ICollection<ApartmentMember> ApartmentMembers { get; set; } = new List<ApartmentMember>();
    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
}