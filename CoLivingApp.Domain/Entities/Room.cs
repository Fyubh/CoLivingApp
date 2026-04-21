using CoLivingApp.Domain.Common;
using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

/// <summary>
/// Комната — приватное (или полуприватное) пространство внутри Apartment.
/// 
/// Зачем нужна отдельно от Apartment, если юнит — студия?
/// - Это место, куда привязывается ContractId/Tenancy (контракт жильца на КОМНАТУ, не на квартиру).
/// - Это scope для NFC-доступа (ключ от комнаты).
/// - Это основа для индивидуальной арендной платы (MonthlyRent у каждой комнаты своя).
/// 
/// Для студии: 1 Apartment содержит 1 Room типа Studio.
/// Для шареной 4BR-квартиры: 1 Apartment содержит 4 Room типа Single.
/// Для двухместного номера (двое делят одну комнату): 1 Room типа Double с двумя жильцами.
/// </summary>
public class Room : EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApartmentId { get; set; }

    /// <summary>Номер или буква комнаты внутри квартиры ("A", "B", "1"). Уникально в пределах квартиры.</summary>
    public string Number { get; set; } = string.Empty;

    public RoomType Type { get; set; } = RoomType.Single;

    public RoomStatus Status { get; set; } = RoomStatus.Available;

    /// <summary>Площадь комнаты в м². Нужна для справок (Potvrzení o ubytování).</summary>
    public decimal? SquareMeters { get; set; }

    /// <summary>
    /// Ежемесячная арендная плата ИМЕННО за эту комнату.
    /// Для студии обычно равна полной цене квартиры.
    /// Для шареной квартиры — цена за одну конкретную комнату.
    /// </summary>
    public decimal? MonthlyRent { get; set; }

    /// <summary>
    /// Максимальное количество жильцов в этой комнате.
    /// Single = 1, Double = 2, Shared = 3+, Studio = 1 (или 2 для пары).
    /// </summary>
    public int MaxOccupancy { get; set; } = 1;

    // Navigation
    public Apartment? Apartment { get; set; }
    // В будущем: public ICollection<Tenancy> Tenancies { get; set; } — для контрактов.
}