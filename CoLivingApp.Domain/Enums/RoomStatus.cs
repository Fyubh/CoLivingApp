namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Текущий статус комнаты для сдачи.
/// Управляется админом здания, влияет на доступность комнаты для заселения.
/// </summary>
public enum RoomStatus
{
    /// <summary>Комната свободна и готова к заселению.</summary>
    Available = 1,

    /// <summary>В комнате живёт активный жилец.</summary>
    Occupied = 2,

    /// <summary>Комната выключена из продажи (ремонт, поломка, клининг после выезда).</summary>
    Maintenance = 3,

    /// <summary>Зарезервирована — контракт подписан, но жилец ещё не заселился.</summary>
    Reserved = 4
}