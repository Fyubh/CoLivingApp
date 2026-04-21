namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Специализация подрядчика. Используется вместе со StaffRole.Contractor для маршрутизации:
/// заявка категории Plumbing ищет доступного подрядчика со Specialization == Plumbing.
/// Nullable на StaffAssignment — не каждый Staff это Contractor (у Reception нет специализации).
/// </summary>
public enum ContractorType
{
    /// <summary>Уборка и клининг.</summary>
    Cleaning = 1,

    /// <summary>Сантехника — трубы, краны, унитазы.</summary>
    Plumbing = 2,

    /// <summary>Электрика — розетки, освещение, проводка.</summary>
    Electric = 3,

    /// <summary>HVAC — отопление, вентиляция, кондиционеры.</summary>
    Hvac = 4,

    /// <summary>Мебель — сборка, ремонт, замена.</summary>
    Furniture = 5,

    /// <summary>Окна и двери.</summary>
    WindowsAndDoors = 6,

    /// <summary>Бытовая техника — стиралки, холодильники, плиты.</summary>
    Appliance = 7,

    /// <summary>Общий ремонт — всё, что не попало в специализации выше.</summary>
    GeneralMaintenance = 8
}