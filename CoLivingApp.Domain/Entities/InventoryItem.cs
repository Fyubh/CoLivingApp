using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public Guid? CatalogItemId { get; set; }
    public string? CustomName { get; set; }
    public decimal Quantity { get; set; } = 1;
    public UnitType Unit { get; set; } = UnitType.Piece;
    public ItemStatus Status { get; set; } = ItemStatus.Available;
    
    // --- НОВЫЕ ПОЛЯ ---
    public ItemCategory Category { get; set; } = ItemCategory.Food;
    public StorageLocation Location { get; set; } = StorageLocation.Fridge;
    public DateTime? ExpiryDate { get; set; } // Срок годности (может быть null)
    // ------------------

    public string CreatedById { get; set; } = string.Empty;
    public DateTime? PurchasedAt { get; set; }
    public string UpdatedById { get; set; } = string.Empty;

    public Apartment? Apartment { get; set; }
    public ProductCatalog? CatalogItem { get; set; }
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}