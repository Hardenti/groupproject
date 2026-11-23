using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class DistributionLine
{
    public int DistributionLineId { get; set; }
    public int DistributionId { get; set; }
    public Distribution? Distribution { get; set; }
    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    public int InventoryLotId { get; set; }
    public InventoryLot? InventoryLot { get; set; }

    [Range(0.01, 100000)]
    public decimal Quantity { get; set; }
}
