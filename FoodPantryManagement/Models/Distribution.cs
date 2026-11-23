using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class Distribution
{
    public int DistributionId { get; set; }
    public int HouseholdId { get; set; }
    public Household? Household { get; set; }
    public int PantrySiteId { get; set; }
    public PantrySite? PantrySite { get; set; }
    public DateTime DistributedAt { get; set; } = DateTime.Now;

    [StringLength(300)]
    public string? Notes { get; set; }

    public ICollection<DistributionLine> Lines { get; set; } = new List<DistributionLine>();
}
