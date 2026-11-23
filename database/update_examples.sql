-- Transactional update examples. Run after schema.sql and sample_data.sql.
PRAGMA foreign_keys = ON;
BEGIN;

-- Update a household record without replacing its transaction history.
UPDATE Households
SET HouseholdSize = 4,
    Notes = 'Updated household size for demonstration'
WHERE HouseholdId = 1;

-- Correct a physical count while preserving the original lot receipt quantity.
UPDATE InventoryLots
SET QuantityRemaining = 28
WHERE InventoryLotId = 1
  AND QuantityRemaining >= 28
  AND QuantityReceived >= 28;

COMMIT;
