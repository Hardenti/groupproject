using System.Diagnostics;
using FoodPantryManagement.Data;
using FoodPantryManagement.Models;
using FoodPantryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodPantryManagement.Controllers;

public sealed class HomeController(PantryDbContext db, OpenMeteoService weather) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<InventoryLot> lots = await db.InventoryLots
            .Include(lot => lot.FoodItem)
            .Where(lot => lot.QuantityRemaining > 0)
            .ToListAsync(cancellationToken);

        List<StockSummary> stock = lots
            .GroupBy(lot => new { lot.FoodItemId, Name = lot.FoodItem!.Name, lot.FoodItem.Unit })
            .Select(group => new StockSummary(
                group.Key.Name,
                group.Key.Unit,
                group.Sum(lot => lot.QuantityRemaining),
                group.Where(lot => lot.ExpiresOn.HasValue)
                    .Select(lot => lot.ExpiresOn)
                    .Min()))
            .OrderBy(item => item.Quantity)
            .ToList();

        DateTime today = DateTime.Today;
        DateTime tomorrow = today.AddDays(1);
        DashboardViewModel model = new()
        {
            ActiveFoodCount = await db.FoodItems.CountAsync(item => item.IsActive, cancellationToken),
            HouseholdCount = await db.Households.CountAsync(cancellationToken),
            LowStockItems = stock.Where(item => item.Quantity <= 5).Take(5).ToList(),
            LowStockCount = stock.Count(item => item.Quantity <= 5),
            ExpiringLotCount = await db.InventoryLots.CountAsync(
                lot => lot.QuantityRemaining > 0
                    && lot.ExpiresOn >= today
                    && lot.ExpiresOn < today.AddDays(8),
                cancellationToken),
            DonationsThisMonth = await db.Donations.CountAsync(
                donation => donation.ReceivedAt >= new DateTime(today.Year, today.Month, 1)
                    && donation.ReceivedAt < new DateTime(today.Year, today.Month, 1).AddMonths(1),
                cancellationToken),
            UpcomingShiftCount = await db.VolunteerShifts.CountAsync(
                shift => shift.StartsAt >= DateTime.Now && shift.StartsAt < tomorrow.AddDays(7),
                cancellationToken),
            RecentDistributions = await db.Distributions
                .OrderByDescending(distribution => distribution.DistributedAt)
                .Take(5)
                .Select(distribution => new RecentDistribution(
                    distribution.Household!.PrimaryContactName,
                    distribution.DistributedAt,
                    distribution.Lines.Count))
                .ToListAsync(cancellationToken)
        };

        PantrySite? site = await db.PantrySites.OrderBy(item => item.PantrySiteId).FirstOrDefaultAsync(cancellationToken);
        if (site is not null)
        {
            model = new DashboardViewModel
            {
                ActiveFoodCount = model.ActiveFoodCount,
                HouseholdCount = model.HouseholdCount,
                LowStockItems = model.LowStockItems,
                LowStockCount = model.LowStockCount,
                ExpiringLotCount = model.ExpiringLotCount,
                DonationsThisMonth = model.DonationsThisMonth,
                UpcomingShiftCount = model.UpcomingShiftCount,
                RecentDistributions = model.RecentDistributions,
                Weather = await weather.GetForecastAsync(site, cancellationToken)
            };
        }

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        Problem("An unexpected error occurred while processing the request.");
}
