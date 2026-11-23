using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class FoodItemsController(PantryDbContext db) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "name", CancellationToken cancellationToken = default)
    {
        IQueryable<FoodItem> query = Db.FoodItems.Include(item => item.FoodCategory);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(item => item.Name.Contains(term) || item.FoodCategory!.Name.Contains(term));
        }

        query = sort switch
        {
            "category" => query.OrderBy(item => item.FoodCategory!.Name).ThenBy(item => item.Name),
            "name-desc" => query.OrderByDescending(item => item.Name),
            _ => query.OrderBy(item => item.Name)
        };

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadCategoriesAsync(cancellationToken: cancellationToken);
        return View(new FoodItem());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FoodItem item, CancellationToken cancellationToken)
    {
        if (await Db.FoodCategories.AllAsync(category => category.FoodCategoryId != item.FoodCategoryId, cancellationToken))
        {
            ModelState.AddModelError(nameof(item.FoodCategoryId), "Choose a valid food category.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(item.FoodCategoryId, cancellationToken);
            return View(item);
        }

        Db.FoodItems.Add(item);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Food item added to the catalog.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return NotFound();
        }

        FoodItem? item = await Db.FoodItems.FindAsync([id.Value], cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        await LoadCategoriesAsync(item.FoodCategoryId, cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FoodItem input, CancellationToken cancellationToken)
    {
        if (id != input.FoodItemId)
        {
            return NotFound();
        }

        FoodItem? item = await Db.FoodItems.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!await Db.FoodCategories.AnyAsync(category => category.FoodCategoryId == input.FoodCategoryId, cancellationToken))
        {
            ModelState.AddModelError(nameof(input.FoodCategoryId), "Choose a valid food category.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(input.FoodCategoryId, cancellationToken);
            return View(input);
        }

        item.Name = input.Name;
        item.Unit = input.Unit;
        item.FoodCategoryId = input.FoodCategoryId;
        item.IsActive = input.IsActive;
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Food item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        FoodItem? item = await Db.FoodItems.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        bool isReferenced = await Db.InventoryLots.AnyAsync(lot => lot.FoodItemId == id, cancellationToken)
            || await Db.DistributionLines.AnyAsync(line => line.FoodItemId == id, cancellationToken);
        if (isReferenced)
        {
            TempData["Error"] = "This item is referenced by pantry records. Mark it inactive instead of deleting it.";
            return RedirectToAction(nameof(Index));
        }

        Db.FoodItems.Remove(item);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Food item deleted.";
        return RedirectToAction(nameof(Index));
    }
}
