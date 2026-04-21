namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Роль сотрудника ВНУТРИ конкретного здания.
/// 
/// Важно: это не глобальная роль пользователя (у User.Role всё ещё есть глобальная Role),
/// а назначение на здание. Один человек может быть BuildingAdmin в здании A
/// и Reception в здании B одновременно — через два разных StaffAssignment.
/// </summary>
public enum StaffRole
{
    /// <summary>Администратор здания — полный контроль (инциденты, ремонт, контракты).</summary>
    BuildingAdmin = 1,

    /// <summary>Ресепшен — заселение/выселение, справки, посылки. Без финансов.</summary>
    Reception = 2,

    /// <summary>Клинер — получает задачи на уборку, отмечает выполнение.</summary>
    Cleaner = 3,

    /// <summary>Подрядчик (сантехник, электрик, мебельщик) — закрывает maintenance-заявки.</summary>
    Contractor = 4,

    /// <summary>Охрана — доступ к access-логам и инцидентам безопасности.</summary>
    Security = 5
}