using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class InventoryLot
{
    public int InventoryLotId { get; set; }
    public int PantrySiteId { get; set; }
    public PantrySite? PantrySite { get; set; }
    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    public int? DonationId { get; set; }
    public Donation? Donation { get; set; }

    [Range(0, 100000)]
    public decimal QuantityReceived { get; set; }

    [Range(0, 100000)]
    public decimal QuantityRemaining { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.Today;
    public DateTime? ExpiresOn { get; set; }
}
