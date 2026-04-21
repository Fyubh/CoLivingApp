namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Тип комнаты внутри квартиры.
/// </summary>
public enum RoomType
{
    /// <summary>Одноместная комната (приватная для одного жильца).</summary>
    Single = 1,

    /// <summary>Двухместная комната (два жильца делят одну комнату).</summary>
    Double = 2,

    /// <summary>Общая спальня (dorm-style, 3+ жильца).</summary>
    Shared = 3,

    /// <summary>Студия — вся квартира является одной "комнатой". 1 Apartment = 1 Room.</summary>
    Studio = 4
}