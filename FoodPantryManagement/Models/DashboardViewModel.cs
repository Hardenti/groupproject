namespace FoodPantryManagement.Models;

public sealed class DashboardViewModel
{
    public int ActiveFoodCount { get; init; }
    public int HouseholdCount { get; init; }
    public int LowStockCount { get; init; }
    public int ExpiringLotCount { get; init; }
    public int DonationsThisMonth { get; init; }
    public int UpcomingShiftCount { get; init; }
    public IReadOnlyList<StockSummary> LowStockItems { get; init; } = [];
    public IReadOnlyList<RecentDistribution> RecentDistributions { get; init; } = [];
    public WeatherSummary? Weather { get; init; }
}

public sealed record StockSummary(string ItemName, string Unit, decimal Quantity, DateTime? NearestExpiry);
public sealed record RecentDistribution(string HouseholdName, DateTime DistributedAt, int ItemCount);

public sealed class WeatherSummary
{
    public string Location { get; init; } = string.Empty;
    public IReadOnlyList<WeatherDay> Days { get; init; } = [];
    public string? Error { get; init; }
}

public sealed record WeatherDay(DateOnly Date, double MaximumCelsius, int? PrecipitationChance, int? WeatherCode);
