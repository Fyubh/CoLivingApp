using CoLivingApp.Domain.Common;
using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Заявка на ремонт/maintenance. Создаётся жильцом, обрабатывается админом и подрядчиком.
/// 
/// Место заявки описывается тремя опциональными полями, заполненными по каскаду:
/// - RoomId заполнено → проблема в приватной комнате жильца (течёт кран в его ванной).
/// - ApartmentId заполнено (без RoomId) → проблема в общей зоне квартиры (кухня 4BR-юнита).
/// - Только BuildingId → проблема в общей зоне здания (коридор, лифт, лобби).
/// BuildingId заполнен ВСЕГДА — это scope для назначения подрядчика.
/// 
/// Денормализация BuildingId (можно было бы получить через Room → Apartment → Floor → Building)
/// нужна для быстрых запросов "все заявки здания X", "все заявки подрядчика за период" без 4 JOIN'ов.
/// </summary>
public class MaintenanceRequest : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ===== ГДЕ =====

    /// <summary>Здание, в котором проблема. Заполнен всегда.</summary>
    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }

    /// <summary>Квартира — если проблема внутри юнита (своя или общая).</summary>
    public Guid? ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }

    /// <summary>Комната — если проблема в приватной комнате жильца.</summary>
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }

    // ===== КТО =====

    /// <summary>Жилец, который создал заявку.</summary>
    public string ReportedByUserId { get; set; } = string.Empty;
    public User? ReportedByUser { get; set; }

    /// <summary>
    /// StaffAssignment подрядчика, которому назначена задача.
    /// Null до стадии Assigned.
    /// </summary>
    public Guid? AssignedStaffAssignmentId { get; set; }
    public StaffAssignment? AssignedStaffAssignment { get; set; }

    // ===== ЧТО =====

    public MaintenanceCategory Category { get; set; }
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Normal;

    /// <summary>Короткий заголовок ("Течёт кран в ванной").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Подробное описание от жильца.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>URL фото "до" от жильца. Загрузка — в Задаче #2.5 (следующая сессия).</summary>
    public string? PhotoUrl { get; set; }

    // ===== WORKFLOW =====

    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Reported;

    /// <summary>Когда админ подтвердил заявку (перевёл из Reported в Acknowledged).</summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>Когда заявка была назначена подрядчику.</summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>Когда подрядчик взял задачу в работу.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Когда задача закрыта.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>URL фото "после" от подрядчика.</summary>
    public string? CompletionPhotoUrl { get; set; }

    /// <summary>Комментарий подрядчика при закрытии ("заменил прокладку, всё ок").</summary>
    public string? CompletionNotes { get; set; }

    // ===== FEEDBACK =====

    /// <summary>Оценка жильца после закрытия (1–5). Влияет на AverageRating подрядчика.</summary>
    public int? ResidentRating { get; set; }

    /// <summary>Текстовый отзыв жильца (опционально).</summary>
    public string? ResidentFeedback { get; set; }
}