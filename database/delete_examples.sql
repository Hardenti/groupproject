-- Demonstrate a safe delete of a donor with no donation history.
-- Historical donors, households, food items, sites, and stock lots are protected
-- by foreign keys when transaction records refer to them.
PRAGMA foreign_keys = ON;
BEGIN;

INSERT INTO Donors (Name, Email, Phone)
VALUES ('Temporary Delete Demo', 'delete-demo@example.org', NULL);

DELETE FROM Donors
WHERE Email = 'delete-demo@example.org';

COMMIT;
