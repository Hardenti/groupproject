using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class InventoryController(PantryDbContext db) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "expiry", CancellationToken cancellationToken = default)
    {
        IQueryable<InventoryLot> query = Db.InventoryLots
            .Include(lot => lot.FoodItem)
            .Include(lot => lot.PantrySite)
            .Where(lot => lot.QuantityRemaining > 0);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(lot => lot.FoodItem!.Name.Contains(term)
                || lot.PantrySite!.Name.Contains(term));
        }

        query = sort switch
        {
            "item" => query.OrderBy(lot => lot.FoodItem!.Name).ThenBy(lot => lot.ExpiresOn),
            "quantity" => query.OrderBy(lot => lot.QuantityRemaining),
            _ => query.OrderBy(lot => lot.ExpiresOn == null).ThenBy(lot => lot.ExpiresOn)
        };

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadFoodItemsAsync(cancellationToken: cancellationToken);
        await LoadSitesAsync(cancellationToken: cancellationToken);
        return View(new InventoryLot { ReceivedAt = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryLot lot, CancellationToken cancellationToken)
    {
        if (lot.QuantityRemaining > lot.QuantityReceived)
        {
            ModelState.AddModelError(nameof(lot.QuantityRemaining), "On-hand quantity cannot exceed the received quantity.");
        }

        if (!await Db.FoodItems.AnyAsync(item => item.FoodItemId == lot.FoodItemId && item.IsActive, cancellationToken))
        {
            ModelState.AddModelError(nameof(lot.FoodItemId), "Choose an active food item.");
        }

        if (!await Db.PantrySites.AnyAsync(site => site.PantrySiteId == lot.PantrySiteId, cancellationToken))
        {
            ModelState.AddModelError(nameof(lot.PantrySiteId), "Choose a valid pantry site.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFoodItemsAsync(lot.FoodItemId, cancellationToken);
            await LoadSitesAsync(lot.PantrySiteId, cancellationToken);
            return View(lot);
        }

        Db.InventoryLots.Add(lot);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Inventory lot recorded.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return NotFound();
        }

        InventoryLot? lot = await Db.InventoryLots
            .Include(item => item.FoodItem)
            .Include(item => item.PantrySite)
            .FirstOrDefaultAsync(item => item.InventoryLotId == id, cancellationToken);
        return lot is null ? NotFound() : View(lot);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, decimal quantityRemaining, DateTime? expiresOn, CancellationToken cancellationToken)
    {
        InventoryLot? lot = await Db.InventoryLots.FindAsync([id], cancellationToken);
        if (lot is null)
        {
            return NotFound();
        }

        decimal alreadyDistributed = await Db.DistributionLines
            .Where(line => line.InventoryLotId == id)
            .SumAsync(line => (decimal?)line.Quantity, cancellationToken) ?? 0;
        if (quantityRemaining < alreadyDistributed || quantityRemaining > lot.QuantityReceived)
        {
            ModelState.AddModelError(nameof(quantityRemaining),
                $"On-hand quantity must be between {alreadyDistributed:0.##} (already distributed) and {lot.QuantityReceived:0.##}.");
        }

        if (!ModelState.IsValid)
        {
            await Db.Entry(lot).Reference(item => item.FoodItem).LoadAsync(cancellationToken);
            await Db.Entry(lot).Reference(item => item.PantrySite).LoadAsync(cancellationToken);
            lot.QuantityRemaining = quantityRemaining;
            lot.ExpiresOn = expiresOn;
            return View(lot);
        }

        lot.QuantityRemaining = quantityRemaining;
        lot.ExpiresOn = expiresOn;
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Inventory lot updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        InventoryLot? lot = await Db.InventoryLots.FindAsync([id], cancellationToken);
        if (lot is null)
        {
            return NotFound();
        }

        if (await Db.DistributionLines.AnyAsync(line => line.InventoryLotId == id, cancellationToken))
        {
            TempData["Error"] = "This lot has distribution history and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        Db.InventoryLots.Remove(lot);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Inventory lot deleted.";
        return RedirectToAction(nameof(Index));
    }
}
