using FoodPantryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Data;

public static class PantrySeed
{
    public static async Task InitializeAsync(PantryDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.PantrySites.AnyAsync(cancellationToken))
        {
            return;
        }

        PantrySite site = new()
        {
            Name = "Community Pantry",
            Address = "100 Main Street",
            City = "Springfield",
            StateCode = "IL",
            PostalCode = "62701",
            Latitude = 39.7817,
            Longitude = -89.6501
        };
        FoodCategory produce = new() { Name = "Produce", RequiresRefrigeration = true };
        FoodCategory pantry = new() { Name = "Pantry staples" };
        FoodCategory canned = new() { Name = "Canned goods" };
        FoodCategory dairy = new() { Name = "Dairy", RequiresRefrigeration = true };
        Donor donor = new()
        {
            Name = "Neighborhood Market",
            Email = "donations@example.org",
            Phone = "555-0100"
        };
        Household household = new()
        {
            PrimaryContactName = "Sample Household",
            Email = "household@example.org",
            HouseholdSize = 3
        };
        FoodItem apples = new() { Name = "Apples", Unit = "lb", FoodCategory = produce };
        FoodItem rice = new() { Name = "Rice", Unit = "lb", FoodCategory = pantry };
        FoodItem beans = new() { Name = "Canned beans", Unit = "can", FoodCategory = canned };
        FoodItem milk = new() { Name = "Shelf-stable milk", Unit = "carton", FoodCategory = dairy };
        Volunteer volunteer = new() { Name = "Sample Volunteer", Email = "volunteer@example.org" };
        Donation donation = new()
        {
            Donor = donor,
            PantrySite = site,
            ReceivedAt = DateTime.Today,
            Notes = "Welcome sample stock"
        };
        donation.Lots.Add(new InventoryLot
        {
            PantrySite = site,
            FoodItem = apples,
            QuantityReceived = 30,
            QuantityRemaining = 30,
            ReceivedAt = DateTime.Today,
            ExpiresOn = DateTime.Today.AddDays(10)
        });
        donation.Lots.Add(new InventoryLot
        {
            PantrySite = site,
            FoodItem = rice,
            QuantityReceived = 50,
            QuantityRemaining = 50,
            ReceivedAt = DateTime.Today
        });
        donation.Lots.Add(new InventoryLot
        {
            PantrySite = site,
            FoodItem = beans,
            QuantityReceived = 48,
            QuantityRemaining = 48,
            ReceivedAt = DateTime.Today,
            ExpiresOn = DateTime.Today.AddMonths(18)
        });

        db.AddRange(site, produce, pantry, canned, dairy, donor, household, apples, rice, beans, milk, volunteer, donation);
        await db.SaveChangesAsync(cancellationToken);

        db.VolunteerShifts.Add(new VolunteerShift
        {
            VolunteerId = volunteer.VolunteerId,
            PantrySiteId = site.PantrySiteId,
            StartsAt = DateTime.Today.AddDays(2).AddHours(9),
            EndsAt = DateTime.Today.AddDays(2).AddHours(12),
            Role = "Pickup support"
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
