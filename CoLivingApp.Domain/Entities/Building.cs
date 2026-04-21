using CoLivingApp.Domain.Common;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Здание — корневая единица физического пространства в B2B-режиме.
/// Все инциденты, ремонты, бронирования, NFC-доступы, посылки — scoped под Building.
/// 
/// Админ видит "своё здание" (или несколько, если назначен на них).
/// Резидент видит только данные своего здания.
/// Подрядчик (клинер, сантехник) получает задачи только из зданий, где он in-shift.
/// </summary>
public class Building : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Оператор-владелец здания.</summary>
    public Guid OperatorId { get; set; }

    /// <summary>Отображаемое название ("The Fizz Prague").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Улица и номер дома ("Jateční 1530/37").</summary>
    public string AddressLine { get; set; } = string.Empty;

    /// <summary>Город ("Prague").</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Двухбуквенный ISO-код страны ("CZ", "DE").</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Почтовый индекс.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Координаты для карты и геозоны NFC.</summary>
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>Часовой пояс здания в формате IANA ("Europe/Prague"). Нужен для локальных напоминаний.</summary>
    public string? TimeZone { get; set; }

    /// <summary>Общее количество этажей (denormalized для быстрых списков в админке).</summary>
    public int TotalFloors { get; set; }

    /// <summary>
    /// Кэш: общее количество квартир в здании. Обновляется при добавлении/удалении Apartment
    /// или фоновой джобой. Нужно, чтобы на дашборде показывать "539 квартир" без тяжёлого COUNT.
    /// </summary>
    public int TotalApartments { get; set; }

    /// <summary>Здание активно (принимает резидентов, видно в мобильном приложении).</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public Operator? Operator { get; set; }
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}