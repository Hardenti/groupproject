using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class VolunteerShift
{
    public int VolunteerShiftId { get; set; }
    public int VolunteerId { get; set; }
    public Volunteer? Volunteer { get; set; }
    public int PantrySiteId { get; set; }
    public PantrySite? PantrySite { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    [Required, StringLength(80)]
    public string Role { get; set; } = string.Empty;
}
