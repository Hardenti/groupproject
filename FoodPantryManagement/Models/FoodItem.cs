using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class FoodItem
{
    public int FoodItemId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Unit { get; set; } = "each";

    public int FoodCategoryId { get; set; }
    public FoodCategory? FoodCategory { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<InventoryLot> InventoryLots { get; set; } = new List<InventoryLot>();
    public ICollection<DistributionLine> DistributionLines { get; set; } = new List<DistributionLine>();
}
