namespace CoLivingApp.Application.Features.Apartments.Queries.GetMyApartmentContext;

/// <summary>
/// Контекст проживания жильца для клиентских форм (например, "где проблема?" в MaintenanceRequest).
/// Возвращает всё, что нужно, одним запросом: ApartmentId (всегда), BuildingId (для B2B-юнита),
/// список Rooms (если шареная квартира — жилец выбирает свою), название здания и номер юнита для UI.
/// 
/// Для consumer-mode квартир BuildingId = null, Rooms = пустой список — клиент показывает
/// только "В квартире" без выбора комнаты и без выбора "В здании".
/// </summary>
public record MyApartmentContextDto(
    Guid ApartmentId,
    string ApartmentName,
    string? UnitNumber,
    Guid? BuildingId,
    string? BuildingName,
    List<RoomOptionDto> Rooms);

/// <summary>Опция выбора комнаты в дропдауне.</summary>
public record RoomOptionDto(
    Guid Id,
    string Number,
    string TypeLabel);