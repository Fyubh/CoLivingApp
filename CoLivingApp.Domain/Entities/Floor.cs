using CoLivingApp.Domain.Common;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Этаж здания. Содержит квартиры (Apartments) и опционально общие зоны на этаже
/// (кухня, лаунж, прачечная — флагами ниже, а реальные сущности CommonArea / BookableResource
/// будут добавлены в отдельной задаче).
/// 
/// Привязка этажа нужна для:
/// - AI-инцидентов: "грязь на кухне этажа 3" → флаг HasSharedKitchen должен быть true.
/// - NFC-доступа: временный ключ подрядчика даётся на этаж, не на всё здание.
/// - Автоназначения дежурств по этажной кухне.
/// </summary>
public class Floor : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BuildingId { get; set; }

    /// <summary>
    /// Номер этажа. Может быть отрицательным (-1 = подвал, -2 = парковка).
    /// 0 = ground floor, 1 = первый жилой этаж и т.д.
    /// Уникален в пределах одного Building (индекс в Configuration).
    /// </summary>
    public int Number { get; set; }

    /// <summary>Опциональное человекочитаемое название ("Ground Floor", "Rooftop", "Mezzanine").</summary>
    public string? Name { get; set; }

    /// <summary>На этаже есть общая кухня.</summary>
    public bool HasSharedKitchen { get; set; }

    /// <summary>На этаже есть общий лаунж / комната отдыха.</summary>
    public bool HasSharedLounge { get; set; }

    /// <summary>На этаже есть прачечная.</summary>
    public bool HasLaundry { get; set; }

    // Navigation
    public Building? Building { get; set; }
    public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
}