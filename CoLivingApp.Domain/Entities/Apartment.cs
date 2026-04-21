namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Apartment — юнит пространства, к которому крепятся траты, чорсы, чат, инвентарь.
/// 
/// Два режима существования:
/// 
/// 1) CONSUMER / ROOMMATE MODE (legacy, текущий).
///    FloorId == null, BuildingId == null. Создаётся через /api/Apartments/create.
///    Получает InviteCode — жильцы вступают сами.
///    Пример: две студентки сняли квартиру в городе, пользуются приложением только для раздела трат.
/// 
/// 2) B2B MODE (новый).
///    FloorId != null, BuildingId == FloorId.BuildingId.
///    Создаётся оператором при настройке здания, InviteCode не используется.
///    Жильцы попадают сюда через контракт (Tenancy) при check-in, не сами.
///    Пример: юнит A303 в The Fizz Prague — студия, в ней 1 Room, в котором живёт один жилец.
/// 
/// Все существующие handlers (CreateApartment, JoinApartment, Expenses, Chores и т.д.)
/// продолжают работать в consumer mode без изменений, т.к. новые поля опциональны.
/// </summary>
public class Apartment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // =================================================================
    // НОВЫЕ ПОЛЯ для B2B-режима. Nullable → backward compatible.
    // =================================================================

    /// <summary>
    /// К какому этажу принадлежит юнит. Null для consumer mode.
    /// </summary>
    public Guid? FloorId { get; set; }
    public Floor? Floor { get; set; }

    /// <summary>
    /// Денормализованная ссылка на здание — для быстрой фильтрации без JOIN через Floor.
    /// Инвариант: если FloorId != null, BuildingId == Floor.BuildingId.
    /// Заполняется при создании Apartment в B2B-режиме.
    /// </summary>
    public Guid? BuildingId { get; set; }
    public Building? Building { get; set; }

    /// <summary>
    /// Номер юнита в здании ("A303", "12B"). Null для consumer mode.
    /// </summary>
    public string? UnitNumber { get; set; }

    // =================================================================
    // Navigation properties
    // =================================================================

    public ICollection<ApartmentMember> Members { get; set; } = new List<ApartmentMember>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    /// <summary>
    /// Комнаты внутри квартиры. Для consumer mode может быть пустым.
    /// Для B2B mode всегда содержит минимум одну Room (для студии — одну типа Studio).
    /// </summary>
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}