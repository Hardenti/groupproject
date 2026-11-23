using System.ComponentModel.DataAnnotations;

namespace FoodPantryManagement.Models;

public sealed class Volunteer
{
    public int VolunteerId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    public ICollection<VolunteerShift> Shifts { get; set; } = new List<VolunteerShift>();
}
