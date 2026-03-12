using CoLivingApp.Domain.Enums;

namespace CoLivingApp.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApartmentId { get; set; }
    public Guid? CatalogItemId { get; set; } // NULL, если товар уникальный
    public string? CustomName { get; set; }
    public decimal Quantity { get; set; } = 1;
    public UnitType Unit { get; set; } = UnitType.Piece;
    public ItemStatus Status { get; set; } = ItemStatus.Available;
    
    public string CreatedById { get; set; } = string.Empty;
    public DateTime? PurchasedAt { get; set; } // Дата перевода в Available
    public string UpdatedById { get; set; } = string.Empty;

    public Apartment? Apartment { get; set; }
    public ProductCatalog? Catagit add .logItem { get; set; }
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}