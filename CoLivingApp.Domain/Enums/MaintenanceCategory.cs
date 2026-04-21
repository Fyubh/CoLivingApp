namespace CoLivingApp.Domain.Enums;

/// <summary>
/// Категория maintenance-заявки. Определяет, какой ContractorType нужен для её закрытия.
/// Маппинг на ContractorType идёт один-к-одному в большинстве случаев.
/// </summary>
public enum MaintenanceCategory
{
    Plumbing = 1,
    Electric = 2,
    Furniture = 3,
    Appliance = 4,
    Hvac = 5,
    WindowsAndDoors = 6,
    Cleaning = 7,
    Internet = 8,
    Other = 99
}