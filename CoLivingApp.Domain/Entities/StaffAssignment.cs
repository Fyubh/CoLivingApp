using CoLivingApp.Domain.Common;
using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Назначение сотрудника на здание. Один User может иметь несколько StaffAssignment:
/// — Маша: BuildingAdmin в The Fizz Prague + Reception в The Fizz Brno.
/// — Петр: Contractor (Plumbing) в трёх разных зданиях, обслуживает их все.
/// 
/// Авторизация по-хорошему должна проходить через эту сущность:
/// "может ли юзер X закрыть заявку Y?" = "есть ли StaffAssignment (X, Y.BuildingId) c правильной Role/Specialization?"
/// 
/// Пока атрибуты [Authorize(Roles=...)] работают на глобальной User.Role — менять это
/// будем, когда появится policy-based авторизация (отдельная будущая задача).
/// </summary>
public class StaffAssignment : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID пользователя-сотрудника.</summary>
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    /// <summary>В каком здании работает.</summary>
    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }

    /// <summary>Роль в этом здании.</summary>
    public StaffRole Role { get; set; }

    /// <summary>
    /// Специализация — заполняется только для Role == Contractor.
    /// Для BuildingAdmin, Reception, Cleaner, Security остаётся null.
    /// </summary>
    public ContractorType? Specialization { get; set; }

    /// <summary>
    /// Сейчас сотрудник на смене (пуши о новых задачах приходят только тем, кто IsOnShift).
    /// Переключается вручную через мобильное приложение сотрудника (check-in / check-out).
    /// </summary>
    public bool IsOnShift { get; set; }

    /// <summary>Когда сотрудник в последний раз встал на смену.</summary>
    public DateTime? ShiftStartedAt { get; set; }

    /// <summary>
    /// Средний рейтинг сотрудника в этом здании (1.0–5.0).
    /// Считается по отзывам жильцов после закрытия задач.
    /// Nullable пока нет ни одной оценки.
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>Сколько задач закрыл в этом здании. Увеличивается каждый раз при CompleteMaintenance.</summary>
    public int CompletedTasksCount { get; set; }

    /// <summary>
    /// Назначение активно. Можно деактивировать (уволен, перевёлся), не удаляя запись —
    /// история задач, которые он закрыл, должна сохраниться.
    /// </summary>
    public bool IsActive { get; set; } = true;
}