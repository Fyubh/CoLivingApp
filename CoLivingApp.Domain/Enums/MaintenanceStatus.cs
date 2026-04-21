namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Статус maintenance-заявки. Workflow:
/// Reported → Acknowledged → Assigned → InProgress → Completed.
/// Возможны ветки: Cancelled (отменена жильцом), Rejected (отклонена админом как не-наша-зона).
/// </summary>
public enum MaintenanceStatus
{
    /// <summary>Жилец создал заявку, никто её ещё не видел.</summary>
    Reported = 1,

    /// <summary>Админ здания увидел заявку и принял к обработке (но ещё не назначил подрядчика).</summary>
    Acknowledged = 2,

    /// <summary>Подрядчику назначена задача, но он ещё не начал работу.</summary>
    Assigned = 3,

    /// <summary>Подрядчик взял задачу в работу (accept + пошёл на место).</summary>
    InProgress = 4,

    /// <summary>Задача закрыта, фото "после" прикреплено.</summary>
    Completed = 5,

    /// <summary>Отменена жильцом (например, чинил сосед, или это был дубль).</summary>
    Cancelled = 6,

    /// <summary>Отклонена админом (не зона ответственности оператора, либо ложная).</summary>
    Rejected = 7
}