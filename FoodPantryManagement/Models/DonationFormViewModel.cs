using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodPantryManagement.Models;

public sealed class DonationFormViewModel
{
    public int? DonationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a donor.")]
    public int DonorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a pantry site.")]
    public int PantrySiteId { get; set; }

    [DataType(DataType.Date)]
    public DateTime ReceivedAt { get; set; } = DateTime.Today;

    [StringLength(300)]
    public string? Notes { get; set; }

    public List<DonationItemInput> Items { get; set; } = [];
    public IReadOnlyList<SelectListItem> Donors { get; set; } = [];
    public IReadOnlyList<SelectListItem> PantrySites { get; set; } = [];
}

public sealed class DonationItemInput
{
    public int FoodItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Quantity { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BestBefore { get; set; }
}
