namespace CoLivingApp.Domain.Common;

/// <summary>
/// Базовый класс для всех entity, которым нужен аудит и soft delete.
/// 
/// ВАЖНО: старые entities (Apartment, User, Expense и т.д.) НЕ наследуют EntityBase,
/// чтобы не ломать существующую логику. Наследуют только новые: Operator, Building, Floor, Room.
/// Миграция старых entities на EntityBase — отдельная задача будущих фаз.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Когда запись была создана (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Когда запись была последний раз изменена (UTC). Null если не менялась.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>UserId того, кто создал запись. Null для системных записей (seed, background jobs).</summary>
    public string? CreatedById { get; set; }

    /// <summary>UserId того, кто последним менял запись.</summary>
    public string? UpdatedById { get; set; }

    /// <summary>
    /// Soft delete: если true — запись считается удалённой, но физически остаётся в БД.
    /// Настраивается через EF Core Global Query Filter в Configuration классе:
    ///   builder.HasQueryFilter(x =&gt; !x.IsDeleted);
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Когда запись была soft-deleted. Заполняется только если IsDeleted == true.</summary>
    public DateTime? DeletedAt { get; set; }
}