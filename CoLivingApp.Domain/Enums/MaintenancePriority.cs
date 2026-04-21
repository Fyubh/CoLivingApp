namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Приоритет maintenance-заявки. Urgent вызывает немедленный push всем подрядчикам
/// с подходящей специализацией, даже если они не IsOnShift.
/// </summary>
public enum MaintenancePriority
{
    /// <summary>Мелочь, можно подождать (перегорела лампочка в коридоре).</summary>
    Low = 1,

    /// <summary>Обычный приоритет (дефолт).</summary>
    Normal = 2,

    /// <summary>Важно, нужно сегодня (не закрывается дверь).</summary>
    High = 3,

    /// <summary>Срочно — прорыв трубы, выбитая дверь, короткое замыкание.</summary>
    Urgent = 4
}