namespace CoLivingApp.Application.Features.Apartments.Queries.GetMyApartmentContext;

/// <summary>
/// Контекст проживания жильца для клиентских форм (например, "где проблема?" в MaintenanceRequest)
/// и Профиля. Возвращает всё одним запросом: ApartmentId (всегда), BuildingId (для B2B),
/// список Rooms (если шареная квартира — жилец выбирает свою), название здания, номер юнита,
/// данные текущего пользователя (MeUserId/MeName) и активных соседей (Roommates).
///
/// Solo vs shared mode: фронт определяет режим по Roommates.Count.
/// Если Roommates содержит только запись с IsMe == true — жилец один (solo).
/// Если есть кто-то ещё — shared mode, открывается вкладка «Соседи».
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
    List<RoomOptionDto> Rooms,
    string MeUserId,
    string MeName,
    List<RoommateDto> Roommates);

/// <summary>Опция выбора комнаты в дропдауне.</summary>
public record RoomOptionDto(
    Guid Id,
    string Number,
    string TypeLabel);

/// <summary>
/// Сосед по квартире — активный ApartmentMember.
/// Список включает самого пользователя (IsMe == true), чтобы фронту было удобно
/// показать "Иван (вы)" в общем списке без отдельного запроса /Users/me.
/// </summary>
public record RoommateDto(
    string UserId,
    string Name,
    string? RoomNumber,
    DateTime JoinedAt,
    bool IsMe);
