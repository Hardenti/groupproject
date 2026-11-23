using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class Household
{
    public int HouseholdId { get; set; }

    [Required, StringLength(120)]
    public string PrimaryContactName { get; set; } = string.Empty;

    [EmailAddress, StringLength(160)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [Range(1, 25)]
    public int HouseholdSize { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
}
