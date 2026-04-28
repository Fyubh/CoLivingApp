namespace CoLivingApp.Domain.Entities;

/// <summary>
/// ApartmentMember — связь "пользователь живёт в квартире".
///
/// Два режима использования (как у Apartment):
///
/// 1) CONSUMER MODE: RoomId == null. Жилец просто состоит в Apartment'е,
///    без привязки к конкретной комнате (две подруги сняли студию вместе).
///
/// 2) B2B MODE: RoomId != null. Жилец привязан к конкретной комнате в здании.
///    Создаётся админом при чек-ине через UI онбординга.
///    Используется для проверки занятости комнаты (Room.MaxOccupancy).
///
/// История заселений хранится через IsActive + JoinedAt + LeftAt:
///  - активный жилец: IsActive == true, LeftAt == null
///  - бывший жилец:    IsActive == false, LeftAt == дата выселения
/// При проверке "комната занята" учитываются ТОЛЬКО активные записи.
/// </summary>
public class ApartmentMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ApartmentId { get; set; }

    /// <summary>
    /// Привязка к конкретной комнате. Null для consumer mode.
    /// В B2B mode обязательно заполняется при создании жильца.
    /// </summary>
    public Guid? RoomId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    public User? User { get; set; }
    public Apartment? Apartment { get; set; }
    public Room? Room { get; set; }
}