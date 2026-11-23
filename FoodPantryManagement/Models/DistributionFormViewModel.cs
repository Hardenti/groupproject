using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodPantryManagement.Models;

public sealed class DistributionFormViewModel
{
    public int? DistributionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a household.")]
    public int HouseholdId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a pantry site.")]
    public int PantrySiteId { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime DistributedAt { get; set; } = DateTime.Now;

    [StringLength(300)]
    public string? Notes { get; set; }

    public List<DistributionItemInput> Items { get; set; } = [];
    public IReadOnlyList<SelectListItem> Households { get; set; } = [];
    public IReadOnlyList<SelectListItem> PantrySites { get; set; } = [];
}

public sealed class DistributionItemInput
{
    public int FoodItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Available { get; set; }

    [Range(0, 100000)]
    public decimal Quantity { get; set; }
}
