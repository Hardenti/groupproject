using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class Donation
{
    public int DonationId { get; set; }
    public int DonorId { get; set; }
    public Donor? Donor { get; set; }
    public int PantrySiteId { get; set; }
    public PantrySite? PantrySite { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.Today;

    [StringLength(300)]
    public string? Notes { get; set; }

    public ICollection<InventoryLot> Lots { get; set; } = new List<InventoryLot>();
}
