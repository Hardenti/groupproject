using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class DonorsController(PantryDbContext db) : PantryController(db)
{
    public async Task<IActionResult> Index(string? search, string sort = "name", CancellationToken cancellationToken = default)
    {
        IQueryable<Donor> query = Db.Donors;
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(donor => donor.Name.Contains(term)
                || (donor.Email != null && donor.Email.Contains(term)));
        }

        query = sort == "recent"
            ? query.OrderByDescending(donor => donor.Donations.Max(donation => (DateTime?)donation.ReceivedAt))
            : query.OrderBy(donor => donor.Name);

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        return View(await query.ToListAsync(cancellationToken));
    }

    public IActionResult Create() => View(new Donor());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Donor donor, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(donor);
        }

        Db.Donors.Add(donor);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Donor added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return NotFound();
        }

        Donor? donor = await Db.Donors.FindAsync([id.Value], cancellationToken);
        return donor is null ? NotFound() : View(donor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Donor input, CancellationToken cancellationToken)
    {
        if (id != input.DonorId)
        {
            return NotFound();
        }

        Donor? donor = await Db.Donors.FindAsync([id], cancellationToken);
        if (donor is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        donor.Name = input.Name;
        donor.Email = input.Email;
        donor.Phone = input.Phone;
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Donor updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Donor? donor = await Db.Donors.FindAsync([id], cancellationToken);
        if (donor is null)
        {
            return NotFound();
        }

        if (await Db.Donations.AnyAsync(donation => donation.DonorId == id, cancellationToken))
        {
            TempData["Error"] = "This donor has donation history and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        Db.Donors.Remove(donor);
        await Db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Donor deleted.";
        return RedirectToAction(nameof(Index));
    }
}
