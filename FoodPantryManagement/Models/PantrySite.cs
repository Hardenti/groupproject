using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class PantrySite
{
    public int PantrySiteId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(2, MinimumLength = 2)]
    public string StateCode { get; set; } = string.Empty;

    [StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<InventoryLot> InventoryLots { get; set; } = new List<InventoryLot>();
    public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
    public ICollection<VolunteerShift> VolunteerShifts { get; set; } = new List<VolunteerShift>();
}
