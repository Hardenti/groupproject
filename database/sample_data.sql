PRAGMA foreign_keys = ON;
BEGIN;

INSERT INTO PantrySites (PantrySiteId, Name, Address, City, StateCode, PostalCode, Latitude, Longitude)
VALUES (1, 'Community Pantry', '100 Main Street', 'Springfield', 'IL', '62701', 39.7817, -89.6501);

INSERT INTO FoodCategories (FoodCategoryId, Name, RequiresRefrigeration) VALUES
(1, 'Produce', 1),
(2, 'Pantry staples', 0),
(3, 'Canned goods', 0),
(4, 'Dairy', 1);

INSERT INTO FoodItems (FoodItemId, Name, Unit, FoodCategoryId, IsActive) VALUES
(1, 'Apples', 'lb', 1, 1),
(2, 'Rice', 'lb', 2, 1),
(3, 'Canned beans', 'can', 3, 1),
(4, 'Shelf-stable milk', 'carton', 4, 1);

INSERT INTO Donors (DonorId, Name, Email, Phone)
VALUES (1, 'Neighborhood Market', 'donations@example.org', '555-0100');

INSERT INTO Households (HouseholdId, PrimaryContactName, Email, Phone, HouseholdSize, Notes, CreatedAt)
VALUES (1, 'Sample Household', 'household@example.org', '555-0101', 3, 'Demonstration record only', datetime('now'));

INSERT INTO Volunteers (VolunteerId, Name, Email, Phone)
VALUES (1, 'Sample Volunteer', 'volunteer@example.org', '555-0102');

INSERT INTO Donations (DonationId, DonorId, PantrySiteId, ReceivedAt, Notes)
VALUES (1, 1, 1, date('now'), 'Welcome sample stock');

INSERT INTO InventoryLots
    (InventoryLotId, PantrySiteId, FoodItemId, DonationId, QuantityReceived, QuantityRemaining, ReceivedAt, ExpiresOn)
VALUES
(1, 1, 1, 1, 30, 30, date('now'), date('now', '+10 days')),
(2, 1, 2, 1, 50, 50, date('now'), NULL),
(3, 1, 3, 1, 48, 48, date('now'), date('now', '+18 months'));

INSERT INTO VolunteerShifts (VolunteerShiftId, VolunteerId, PantrySiteId, StartsAt, EndsAt, Role)
VALUES (1, 1, 1, datetime('now', '+2 days', 'start of day', '+9 hours'), datetime('now', '+2 days', 'start of day', '+12 hours'), 'Pickup support');

COMMIT;
