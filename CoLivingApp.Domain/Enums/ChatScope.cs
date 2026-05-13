namespace CoLivingApp.Domain.Enums;

/// <summary>
/// К какому контексту привязано сообщение чата.
/// Один из ApartmentId / BuildingId / EventId должен быть не-null и совпадать со Scope —
/// это инвариант, проверяемый CHECK-constraint в БД.
/// </summary>
public enum ChatScope
{
    /// <summary>Чат внутри квартиры — roommate-layer. ApartmentId != null.</summary>
    Apartment = 1,

    /// <summary>Чат на всё здание — community-layer. BuildingId != null.</summary>
    Building = 2,

    /// <summary>Чат участников эвента — community-layer (8.5). EventId != null.</summary>
    Event = 3
}
