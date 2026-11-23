using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class DonationsController(PantryDbContext db, ILogger<DonationsController> logger) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "recent", CancellationToken cancellationToken = default)
    {
        IQueryable<Donation> query = Db.Donations
            .Include(donation => donation.Donor)
            .Include(donation => donation.PantrySite)
            .Include(donation => donation.Lots)
                .ThenInclude(lot => lot.FoodItem);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(donation => donation.Donor!.Name.Contains(term)
                || donation.Lots.Any(lot => lot.FoodItem!.Name.Contains(term)));
        }

        query = sort == "donor"
            ? query.OrderBy(donation => donation.Donor!.Name).ThenByDescending(donation => donation.ReceivedAt)
            : query.OrderByDescending(donation => donation.ReceivedAt);
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        DonationFormViewModel model = new() { Items = await GetFoodInputsAsync(cancellationToken) };
        await LoadFormListsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonationFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Items.GroupBy(item => item.FoodItemId).Any(group => group.Count() > 1))
        {
            ModelState.AddModelError(nameof(model.Items), "Each food item can appear only once in a donation.");
        }

        HashSet<int> activeFoodIds = (await Db.FoodItems
            .Where(item => item.IsActive)
            .Select(item => item.FoodItemId)
            .ToListAsync(cancellationToken))
            .ToHashSet();
        if (model.Items.Any(item => !activeFoodIds.Contains(item.FoodItemId)))
        {
            ModelState.AddModelError(nameof(model.Items), "One or more selected food items are unavailable. Refresh and try again.");
        }

        model.Items = await MergeFoodNamesAsync(model.Items, cancellationToken);
        bool hasItems = model.Items.Any(item => item.Quantity > 0);
        if (!hasItems)
        {
            ModelState.AddModelError(nameof(model.Items), "Enter a quantity for at least one donated item.");
        }

        if (model.Items.Any(item => item.Quantity < 0))
        {
            ModelState.AddModelError(nameof(model.Items), "Donation quantities cannot be negative.");
        }

        if (model.Items.Any(item => item.BestBefore < DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.Items), "A best-before date cannot be in the past.");
        }

        if (!await Db.Donors.AnyAsync(donor => donor.DonorId == model.DonorId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.DonorId), "Choose a valid donor.");
        }

        if (!await Db.PantrySites.AnyAsync(site => site.PantrySiteId == model.PantrySiteId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PantrySiteId), "Choose a valid pantry site.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        await using var transaction = await Db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Donation donation = new()
            {
                DonorId = model.DonorId,
                PantrySiteId = model.PantrySiteId,
                ReceivedAt = model.ReceivedAt,
                Notes = model.Notes
            };
            foreach (DonationItemInput input in model.Items.Where(item => item.Quantity > 0))
            {
                donation.Lots.Add(new InventoryLot
                {
                    PantrySiteId = model.PantrySiteId,
                    FoodItemId = input.FoodItemId,
                    QuantityReceived = input.Quantity,
                    QuantityRemaining = input.Quantity,
                    ReceivedAt = model.ReceivedAt,
                    ExpiresOn = input.BestBefore
                });
            }

            Db.Donations.Add(donation);
            await Db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(exception, "Could not save donation for donor {DonorId}.", model.DonorId);
            ModelState.AddModelError(string.Empty, "The donation could not be saved. Check the selected donor, site, and items.");
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Donation received and added to inventory.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        Donation? donation = await Db.Donations
            .Include(item => item.Donor)
            .Include(item => item.PantrySite)
            .Include(item => item.Lots)
                .ThenInclude(lot => lot.FoodItem)
            .FirstOrDefaultAsync(item => item.DonationId == id, cancellationToken);
        return donation is null ? NotFound() : View(donation);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Donation? donation = await Db.Donations
            .Include(item => item.Lots)
            .FirstOrDefaultAsync(item => item.DonationId == id, cancellationToken);
        if (donation is null)
        {
            return NotFound();
        }

        if (donation.Lots.Any(lot => lot.QuantityRemaining != lot.QuantityReceived
            || Db.DistributionLines.Any(line => line.InventoryLotId == lot.InventoryLotId)))
        {
            TempData["Error"] = "This donation has stock or distribution history that prevents deletion.";
            return RedirectToAction(nameof(Index));
        }

        Db.InventoryLots.RemoveRange(donation.Lots);
        Db.Donations.Remove(donation);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Donation and its unused stock were deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormListsAsync(DonationFormViewModel model, CancellationToken cancellationToken)
    {
        model.Donors = await Db.Donors.OrderBy(donor => donor.Name)
            .Select(donor => new SelectListItem(donor.Name, donor.DonorId.ToString(), donor.DonorId == model.DonorId))
            .ToListAsync(cancellationToken);
        model.PantrySites = await Db.PantrySites.OrderBy(site => site.Name)
            .Select(site => new SelectListItem(site.Name, site.PantrySiteId.ToString(), site.PantrySiteId == model.PantrySiteId))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DonationItemInput>> GetFoodInputsAsync(CancellationToken cancellationToken) =>
        await Db.FoodItems.Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => new DonationItemInput { FoodItemId = item.FoodItemId, Name = item.Name, Unit = item.Unit })
            .ToListAsync(cancellationToken);

    private async Task<List<DonationItemInput>> MergeFoodNamesAsync(
        IEnumerable<DonationItemInput> submitted,
        CancellationToken cancellationToken)
    {
        Dictionary<int, FoodItem> items = await Db.FoodItems
            .Where(item => item.IsActive)
            .ToDictionaryAsync(item => item.FoodItemId, cancellationToken);
        return submitted
            .Where(input => items.ContainsKey(input.FoodItemId))
            .GroupBy(input => input.FoodItemId)
            .Select(group =>
            {
                FoodItem food = items[group.Key];
                DonationItemInput input = group.First();
                return new DonationItemInput
                {
                    FoodItemId = food.FoodItemId,
                    Name = food.Name,
                    Unit = food.Unit,
                    Quantity = input.Quantity,
                    BestBefore = input.BestBefore
                };
            })
            .ToList();
    }
}
