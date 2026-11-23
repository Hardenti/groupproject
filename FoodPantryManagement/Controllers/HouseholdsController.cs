using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class HouseholdsController(PantryDbContext db) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "name", CancellationToken cancellationToken = default)
    {
        IQueryable<Household> query = Db.Households;
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(item => item.PrimaryContactName.Contains(term)
                || (item.Email != null && item.Email.Contains(term))
                || (item.Phone != null && item.Phone.Contains(term)));
        }

        query = sort switch
        {
            "size" => query.OrderByDescending(item => item.HouseholdSize).ThenBy(item => item.PrimaryContactName),
            "oldest" => query.OrderBy(item => item.CreatedAt),
            _ => query.OrderBy(item => item.PrimaryContactName)
        };

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public IActionResult Create() => View(new Household());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Household household, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(household);
        }

        household.CreatedAt = DateTime.UtcNow;
        Db.Households.Add(household);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Household registered.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return NotFound();
        }

        Household? household = await Db.Households.FindAsync([id.Value], cancellationToken);
        return household is null ? NotFound() : View(household);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Household input, CancellationToken cancellationToken)
    {
        if (id != input.HouseholdId)
        {
            return NotFound();
        }

        Household? household = await Db.Households.FindAsync([id], cancellationToken);
        if (household is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        household.PrimaryContactName = input.PrimaryContactName;
        household.Email = input.Email;
        household.Phone = input.Phone;
        household.HouseholdSize = input.HouseholdSize;
        household.Notes = input.Notes;
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Household details updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Household? household = await Db.Households.FindAsync([id], cancellationToken);
        if (household is null)
        {
            return NotFound();
        }

        if (await Db.Distributions.AnyAsync(item => item.HouseholdId == id, cancellationToken))
        {
            TempData["Error"] = "This household has distribution history and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        Db.Households.Remove(household);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Household deleted.";
        return RedirectToAction(nameof(Index));
    }
}
