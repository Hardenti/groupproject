using FoodPantryManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public abstract class PantryController(PantryDbContext db) : Controller
{
    protected PantryDbContext Db { get; } = db;

    protected async Task LoadCategoriesAsync(int? selectedId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.FoodCategoryId = new SelectList(
            await Db.FoodCategories.OrderBy(category => category.Name).ToListAsync(cancellationToken),
            "FoodCategoryId",
            "Name",
            selectedId);
    }

    protected async Task LoadDonorsAsync(int? selectedId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.DonorId = new SelectList(
            await Db.Donors.OrderBy(donor => donor.Name).ToListAsync(cancellationToken),
            "DonorId",
            "Name",
            selectedId);
    }

    protected async Task LoadHouseholdsAsync(int? selectedId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.HouseholdId = new SelectList(
            await Db.Households.OrderBy(household => household.PrimaryContactName).ToListAsync(cancellationToken),
            "HouseholdId",
            "PrimaryContactName",
            selectedId);
    }

    protected async Task LoadSitesAsync(int? selectedId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.PantrySiteId = new SelectList(
            await Db.PantrySites.OrderBy(site => site.Name).ToListAsync(cancellationToken),
            "PantrySiteId",
            "Name",
            selectedId);
    }

    protected async Task LoadFoodItemsAsync(int? selectedId = null, CancellationToken cancellationToken = default)
    {
        ViewBag.FoodItemId = new SelectList(
            await Db.FoodItems.Where(item => item.IsActive).OrderBy(item => item.Name).ToListAsync(cancellationToken),
            "FoodItemId",
            "Name",
            selectedId);
    }
}
