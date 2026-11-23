PRAGMA foreign_keys = ON;

-- Available quantity by pantry and food item, with category context.
SELECT
    site.Name AS Pantry,
    category.Name AS Category,
    item.Name AS FoodItem,
    item.Unit,
    SUM(lot.QuantityRemaining) AS AvailableQuantity
FROM InventoryLots AS lot
JOIN PantrySites AS site ON site.PantrySiteId = lot.PantrySiteId
JOIN FoodItems AS item ON item.FoodItemId = lot.FoodItemId
JOIN FoodCategories AS category ON category.FoodCategoryId = item.FoodCategoryId
WHERE lot.QuantityRemaining > 0
GROUP BY site.PantrySiteId, category.FoodCategoryId, item.FoodItemId
ORDER BY site.Name, category.Name, item.Name;

-- Lots that expire within the next week, ordered by FEFO.
SELECT
    item.Name AS FoodItem,
    site.Name AS Pantry,
    lot.QuantityRemaining,
    item.Unit,
    lot.ExpiresOn
FROM InventoryLots AS lot
JOIN FoodItems AS item ON item.FoodItemId = lot.FoodItemId
JOIN PantrySites AS site ON site.PantrySiteId = lot.PantrySiteId
WHERE lot.QuantityRemaining > 0
  AND lot.ExpiresOn >= date('now')
  AND lot.ExpiresOn < date('now', '+8 days')
ORDER BY lot.ExpiresOn, item.Name;

-- Donation totals by donor for the last 90 days.
SELECT
    donor.Name AS Donor,
    COUNT(DISTINCT donation.DonationId) AS DonationCount,
    COUNT(lot.InventoryLotId) AS ItemLines,
    ROUND(SUM(lot.QuantityReceived), 2) AS TotalUnits
FROM Donors AS donor
JOIN Donations AS donation ON donation.DonorId = donor.DonorId
JOIN InventoryLots AS lot ON lot.DonationId = donation.DonationId
WHERE donation.ReceivedAt >= datetime('now', '-90 days')
GROUP BY donor.DonorId, donor.Name
ORDER BY TotalUnits DESC, donor.Name;

-- Household support totals with member count and distinct food item count.
SELECT
    household.PrimaryContactName AS Household,
    household.HouseholdSize,
    COUNT(DISTINCT distribution.DistributionId) AS VisitCount,
    COUNT(DISTINCT line.FoodItemId) AS DistinctFoodItems,
    ROUND(SUM(line.Quantity), 2) AS TotalQuantity
FROM Households AS household
LEFT JOIN Distributions AS distribution ON distribution.HouseholdId = household.HouseholdId
LEFT JOIN DistributionLines AS line ON line.DistributionId = distribution.DistributionId
GROUP BY household.HouseholdId, household.PrimaryContactName, household.HouseholdSize
ORDER BY VisitCount DESC, household.PrimaryContactName;

-- Active catalog items that are low or out of stock at each pantry.
SELECT
    site.Name AS Pantry,
    item.Name AS FoodItem,
    item.Unit,
    COALESCE(SUM(lot.QuantityRemaining), 0) AS AvailableQuantity
FROM PantrySites AS site
CROSS JOIN FoodItems AS item
LEFT JOIN InventoryLots AS lot
    ON lot.PantrySiteId = site.PantrySiteId
   AND lot.FoodItemId = item.FoodItemId
WHERE item.IsActive = 1
GROUP BY site.PantrySiteId, item.FoodItemId, item.Name, item.Unit
HAVING COALESCE(SUM(lot.QuantityRemaining), 0) <= 5
ORDER BY AvailableQuantity, site.Name, item.Name;
