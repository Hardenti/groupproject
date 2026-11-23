using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class DistributionsController(
    PantryDbContext db,
    ILogger<DistributionsController> logger) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "recent", CancellationToken cancellationToken = default)
    {
        IQueryable<Distribution> query = Db.Distributions
            .Include(distribution => distribution.Household)
            .Include(distribution => distribution.PantrySite)
            .Include(distribution => distribution.Lines);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(distribution => distribution.Household!.PrimaryContactName.Contains(term)
                || distribution.PantrySite!.Name.Contains(term));
        }

        query = sort == "household"
            ? query.OrderBy(distribution => distribution.Household!.PrimaryContactName)
                .ThenByDescending(distribution => distribution.DistributedAt)
            : query.OrderByDescending(distribution => distribution.DistributedAt);
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        DistributionFormViewModel model = new();
        int? firstSiteId = await Db.PantrySites.OrderBy(site => site.PantrySiteId)
            .Select(site => (int?)site.PantrySiteId).FirstOrDefaultAsync(cancellationToken);
        if (firstSiteId.HasValue)
        {
            model.PantrySiteId = firstSiteId.Value;
        }

        model.Items = await GetFoodInputsAsync(model.PantrySiteId, cancellationToken: cancellationToken);
        await LoadFormListsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DistributionFormViewModel model, CancellationToken cancellationToken)
    {
        List<DistributionItemInput> submitted = model.Items;
        if (submitted.GroupBy(item => item.FoodItemId).Any(group => group.Count() > 1))
        {
            ModelState.AddModelError(nameof(model.Items), "Each food item can appear only once in a distribution.");
        }

        HashSet<int> activeFoodIds = (await Db.FoodItems
            .Where(item => item.IsActive)
            .Select(item => item.FoodItemId)
            .ToListAsync(cancellationToken))
            .ToHashSet();
        if (submitted.Any(item => !activeFoodIds.Contains(item.FoodItemId)))
        {
            ModelState.AddModelError(nameof(model.Items), "One or more selected food items are unavailable. Refresh and try again.");
        }

        await NormalizeItemsAsync(model, submitted, null, cancellationToken);
        if (!model.Items.Any(item => item.Quantity > 0))
        {
            ModelState.AddModelError(nameof(model.Items), "Choose at least one item to distribute.");
        }

        if (!await Db.Households.AnyAsync(item => item.HouseholdId == model.HouseholdId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.HouseholdId), "Choose a valid household.");
        }

        if (!await Db.PantrySites.AnyAsync(item => item.PantrySiteId == model.PantrySiteId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PantrySiteId), "Choose a valid pantry site.");
        }

        ValidateRequestedQuantities(model);
        if (!ModelState.IsValid)
        {
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        await using var transaction = await Db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Distribution distribution = new()
            {
                HouseholdId = model.HouseholdId,
                PantrySiteId = model.PantrySiteId,
                DistributedAt = model.DistributedAt,
                Notes = model.Notes
            };
            Db.Distributions.Add(distribution);
            await Db.SaveChangesAsync(cancellationToken);

            if (!await ApplyRequestedItemsAsync(distribution, model.Items, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                Db.ChangeTracker.Clear();
                ModelState.AddModelError(nameof(model.Items), "Available stock changed while saving. Review the current stock and try again.");
                await NormalizeItemsAsync(model, submitted, null, cancellationToken);
                await LoadFormListsAsync(model, cancellationToken);
                return View(model);
            }

            await Db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            Db.ChangeTracker.Clear();
            logger.LogError(exception, "Could not save household distribution for household {HouseholdId}.", model.HouseholdId);
            ModelState.AddModelError(string.Empty, "The distribution could not be saved. Check the household, pantry site, and stock.");
            await NormalizeItemsAsync(model, submitted, null, cancellationToken);
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Distribution recorded and inventory updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        Distribution? distribution = await Db.Distributions
            .Include(item => item.Lines)
            .FirstOrDefaultAsync(item => item.DistributionId == id, cancellationToken);
        if (distribution is null)
        {
            return NotFound();
        }

        Dictionary<int, decimal> quantities = distribution.Lines
            .GroupBy(line => line.FoodItemId)
            .ToDictionary(group => group.Key, group => group.Sum(line => line.Quantity));
        DistributionFormViewModel model = new()
        {
            DistributionId = distribution.DistributionId,
            HouseholdId = distribution.HouseholdId,
            PantrySiteId = distribution.PantrySiteId,
            DistributedAt = distribution.DistributedAt,
            Notes = distribution.Notes,
            Items = await GetFoodInputsAsync(distribution.PantrySiteId, quantities, id, cancellationToken)
        };
        await LoadFormListsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DistributionFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.DistributionId)
        {
            return NotFound();
        }

        List<DistributionItemInput> submitted = model.Items;
        if (submitted.GroupBy(item => item.FoodItemId).Any(group => group.Count() > 1))
        {
            ModelState.AddModelError(nameof(model.Items), "Each food item can appear only once in a distribution.");
        }

        HashSet<int> activeFoodIds = (await Db.FoodItems
            .Where(item => item.IsActive)
            .Select(item => item.FoodItemId)
            .ToListAsync(cancellationToken))
            .ToHashSet();
        if (submitted.Any(item => !activeFoodIds.Contains(item.FoodItemId)))
        {
            ModelState.AddModelError(nameof(model.Items), "One or more selected food items are unavailable. Refresh and try again.");
        }

        Distribution? distribution = await Db.Distributions
            .Include(item => item.Lines)
                .ThenInclude(line => line.InventoryLot)
            .FirstOrDefaultAsync(item => item.DistributionId == id, cancellationToken);
        if (distribution is null)
        {
            return NotFound();
        }

        await NormalizeItemsAsync(model, submitted, id, cancellationToken);
        if (!model.Items.Any(item => item.Quantity > 0))
        {
            ModelState.AddModelError(nameof(model.Items), "Choose at least one item to distribute.");
        }

        if (!await Db.Households.AnyAsync(item => item.HouseholdId == model.HouseholdId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.HouseholdId), "Choose a valid household.");
        }

        if (!await Db.PantrySites.AnyAsync(item => item.PantrySiteId == model.PantrySiteId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PantrySiteId), "Choose a valid pantry site.");
        }

        ValidateRequestedQuantities(model);
        if (!ModelState.IsValid)
        {
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        await using var transaction = await Db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (DistributionLine line in distribution.Lines)
            {
                if (line.InventoryLot is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    ModelState.AddModelError(string.Empty, "A stock lot for this distribution is missing; the record cannot be edited safely.");
                    await NormalizeItemsAsync(model, submitted, id, cancellationToken);
                    await LoadFormListsAsync(model, cancellationToken);
                    return View(model);
                }

                line.InventoryLot.QuantityRemaining += line.Quantity;
            }

            Db.DistributionLines.RemoveRange(distribution.Lines);
            distribution.HouseholdId = model.HouseholdId;
            distribution.PantrySiteId = model.PantrySiteId;
            distribution.DistributedAt = model.DistributedAt;
            distribution.Notes = model.Notes;
            await Db.SaveChangesAsync(cancellationToken);

            if (!await ApplyRequestedItemsAsync(distribution, model.Items, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                Db.ChangeTracker.Clear();
                ModelState.AddModelError(nameof(model.Items), "Available stock changed while saving. Review the current stock and try again.");
                await NormalizeItemsAsync(model, submitted, id, cancellationToken);
                await LoadFormListsAsync(model, cancellationToken);
                return View(model);
            }

            await Db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            Db.ChangeTracker.Clear();
            logger.LogError(exception, "Could not update distribution {DistributionId}.", id);
            ModelState.AddModelError(string.Empty, "The distribution could not be updated. Check the household, pantry site, and stock.");
            await NormalizeItemsAsync(model, submitted, id, cancellationToken);
            await LoadFormListsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Distribution updated and inventory reconciled.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        Distribution? distribution = await Db.Distributions
            .Include(item => item.Household)
            .Include(item => item.PantrySite)
            .Include(item => item.Lines)
                .ThenInclude(line => line.FoodItem)
            .FirstOrDefaultAsync(item => item.DistributionId == id, cancellationToken);
        return distribution is null ? NotFound() : View(distribution);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Distribution? distribution = await Db.Distributions
            .Include(item => item.Lines)
                .ThenInclude(line => line.InventoryLot)
            .FirstOrDefaultAsync(item => item.DistributionId == id, cancellationToken);
        if (distribution is null)
        {
            return NotFound();
        }

        if (distribution.Lines.Any(line => line.InventoryLot is null))
        {
            TempData["Error"] = "A stock lot for this distribution is missing. Inventory was not changed.";
            return RedirectToAction(nameof(Index));
        }

        foreach (DistributionLine line in distribution.Lines)
        {
            line.InventoryLot!.QuantityRemaining += line.Quantity;
        }

        Db.Distributions.Remove(distribution);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Distribution deleted and stock returned to inventory.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ApplyRequestedItemsAsync(
        Distribution distribution,
        IReadOnlyCollection<DistributionItemInput> inputs,
        CancellationToken cancellationToken)
    {
        foreach (IGrouping<int, DistributionItemInput> itemGroup in inputs
            .Where(item => item.Quantity > 0)
            .GroupBy(item => item.FoodItemId))
        {
            decimal requested = itemGroup.Sum(item => item.Quantity);
            List<InventoryLot> lots = await Db.InventoryLots
                .Where(lot => lot.PantrySiteId == distribution.PantrySiteId
                    && lot.FoodItemId == itemGroup.Key
                    && lot.QuantityRemaining > 0)
                .OrderBy(lot => lot.ExpiresOn == null)
                .ThenBy(lot => lot.ExpiresOn)
                .ThenBy(lot => lot.ReceivedAt)
                .ToListAsync(cancellationToken);

            if (lots.Sum(lot => lot.QuantityRemaining) < requested)
            {
                return false;
            }

            decimal remaining = requested;
            foreach (InventoryLot lot in lots)
            {
                decimal issued = Math.Min(lot.QuantityRemaining, remaining);
                if (issued <= 0)
                {
                    continue;
                }

                lot.QuantityRemaining -= issued;
                distribution.Lines.Add(new DistributionLine
                {
                    FoodItemId = itemGroup.Key,
                    InventoryLotId = lot.InventoryLotId,
                    Quantity = issued
                });
                remaining -= issued;
                if (remaining == 0)
                {
                    break;
                }
            }
        }

        return true;
    }

    private async Task NormalizeItemsAsync(
        DistributionFormViewModel model,
        IEnumerable<DistributionItemInput> submitted,
        int? editingDistributionId,
        CancellationToken cancellationToken)
    {
        Dictionary<int, decimal> quantities = submitted
            .Where(item => item.Quantity > 0)
            .GroupBy(item => item.FoodItemId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));
        model.Items = await GetFoodInputsAsync(
            model.PantrySiteId,
            quantities,
            editingDistributionId,
            cancellationToken);
    }

    private async Task<List<DistributionItemInput>> GetFoodInputsAsync(
        int pantrySiteId,
        IReadOnlyDictionary<int, decimal>? selectedQuantities = null,
        int? editingDistributionId = null,
        CancellationToken cancellationToken = default)
    {
        List<InventoryLot> lots = await Db.InventoryLots
            .Where(lot => lot.PantrySiteId == pantrySiteId && lot.QuantityRemaining > 0)
            .ToListAsync(cancellationToken);
        Dictionary<int, decimal> available = lots
            .GroupBy(lot => lot.FoodItemId)
            .ToDictionary(group => group.Key, group => group.Sum(lot => lot.QuantityRemaining));

        if (editingDistributionId.HasValue)
        {
            List<DistributionLine> oldLines = await Db.DistributionLines
                .Include(line => line.InventoryLot)
                .Where(line => line.DistributionId == editingDistributionId.Value)
                .ToListAsync(cancellationToken);
            foreach (DistributionLine line in oldLines.Where(line => line.InventoryLot?.PantrySiteId == pantrySiteId))
            {
                available[line.FoodItemId] = available.GetValueOrDefault(line.FoodItemId) + line.Quantity;
            }
        }

        List<FoodItem> foodItems = await Db.FoodItems
            .Where(item => item.IsActive)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        return foodItems.Select(item => new DistributionItemInput
        {
            FoodItemId = item.FoodItemId,
            Name = item.Name,
            Unit = item.Unit,
            Available = available.GetValueOrDefault(item.FoodItemId),
            Quantity = selectedQuantities?.GetValueOrDefault(item.FoodItemId) ?? 0
        }).ToList();
    }

    private async Task LoadFormListsAsync(DistributionFormViewModel model, CancellationToken cancellationToken)
    {
        model.Households = await Db.Households.OrderBy(item => item.PrimaryContactName)
            .Select(item => new SelectListItem(item.PrimaryContactName, item.HouseholdId.ToString(), item.HouseholdId == model.HouseholdId))
            .ToListAsync(cancellationToken);
        model.PantrySites = await Db.PantrySites.OrderBy(item => item.Name)
            .Select(item => new SelectListItem(item.Name, item.PantrySiteId.ToString(), item.PantrySiteId == model.PantrySiteId))
            .ToListAsync(cancellationToken);
    }

    private void ValidateRequestedQuantities(DistributionFormViewModel model)
    {
        foreach (DistributionItemInput item in model.Items)
        {
            if (item.Quantity < 0)
            {
                ModelState.AddModelError(nameof(model.Items), "Item quantities cannot be negative.");
            }
            else if (item.Quantity > item.Available)
            {
                ModelState.AddModelError(nameof(model.Items),
                    $"{item.Name}: requested quantity exceeds the available {item.Available:0.##} {item.Unit}.");
            }
        }
    }
}
