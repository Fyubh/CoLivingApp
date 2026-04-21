namespace CoLivingApp.Domain.Enums;

public enum UserRole
{
    Tenant = 1,       // Жилец (Студент)
    Staff = 2,        // Персонал (Клининг, Мастера)
    Admin = 3,        // Администрация (Ресепшен, Комендант)
    SuperAdmin = 4    // Владелец/Генменеджер
}