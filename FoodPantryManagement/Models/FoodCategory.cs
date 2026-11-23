using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class FoodCategory
{
    public int FoodCategoryId { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    public bool RequiresRefrigeration { get; set; }

    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
