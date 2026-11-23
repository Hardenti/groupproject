using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class Donor
{
    public int DonorId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(160)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
