using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class ProductCatalog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsGlobal { get; set; } = true;
    public ItemCategory Category { get; set; } = ItemCategory.Food;
    public int? AverageShelfLifeDays { get; set; } // NULL, если не портится
}