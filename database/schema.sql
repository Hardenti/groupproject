-- Pantry Table schema for SQLite.
-- Load into a fresh database. Foreign-key enforcement is enabled for this connection.
PRAGMA foreign_keys = ON;

CREATE TABLE PantrySites (
    PantrySiteId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 1 AND 100),
    Address TEXT NOT NULL DEFAULT '' CHECK (length(Address) <= 160),
    City TEXT NOT NULL CHECK (length(trim(City)) BETWEEN 1 AND 80),
    StateCode TEXT NOT NULL CHECK (length(StateCode) = 2),
    PostalCode TEXT NOT NULL DEFAULT '' CHECK (length(PostalCode) <= 10),
    Latitude REAL NOT NULL CHECK (Latitude BETWEEN -90 AND 90),
    Longitude REAL NOT NULL CHECK (Longitude BETWEEN -180 AND 180)
);

CREATE TABLE FoodCategories (
    FoodCategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE CHECK (length(trim(Name)) BETWEEN 1 AND 60),
    RequiresRefrigeration INTEGER NOT NULL DEFAULT 0 CHECK (RequiresRefrigeration IN (0, 1))
);

CREATE TABLE FoodItems (
    FoodItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 1 AND 100),
    Unit TEXT NOT NULL CHECK (length(trim(Unit)) BETWEEN 1 AND 30),
    FoodCategoryId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    FOREIGN KEY (FoodCategoryId) REFERENCES FoodCategories(FoodCategoryId) ON DELETE RESTRICT
);
CREATE INDEX IX_FoodItems_FoodCategoryId ON FoodItems(FoodCategoryId);
CREATE INDEX IX_FoodItems_Name ON FoodItems(Name);

CREATE TABLE Donors (
    DonorId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 1 AND 120),
    Email TEXT CHECK (Email IS NULL OR length(Email) <= 160),
    Phone TEXT CHECK (Phone IS NULL OR length(Phone) <= 30)
);

CREATE TABLE Households (
    HouseholdId INTEGER PRIMARY KEY AUTOINCREMENT,
    PrimaryContactName TEXT NOT NULL CHECK (length(trim(PrimaryContactName)) BETWEEN 1 AND 120),
    Email TEXT CHECK (Email IS NULL OR length(Email) <= 160),
    Phone TEXT CHECK (Phone IS NULL OR length(Phone) <= 30),
    HouseholdSize INTEGER NOT NULL CHECK (HouseholdSize BETWEEN 1 AND 25),
    Notes TEXT CHECK (Notes IS NULL OR length(Notes) <= 500),
    CreatedAt TEXT NOT NULL
);
CREATE INDEX IX_Households_PrimaryContactName ON Households(PrimaryContactName);

CREATE TABLE Donations (
    DonationId INTEGER PRIMARY KEY AUTOINCREMENT,
    DonorId INTEGER NOT NULL,
    PantrySiteId INTEGER NOT NULL,
    ReceivedAt TEXT NOT NULL,
    Notes TEXT CHECK (Notes IS NULL OR length(Notes) <= 300),
    FOREIGN KEY (DonorId) REFERENCES Donors(DonorId) ON DELETE RESTRICT,
    FOREIGN KEY (PantrySiteId) REFERENCES PantrySites(PantrySiteId) ON DELETE RESTRICT
);
CREATE INDEX IX_Donations_DonorId ON Donations(DonorId);
CREATE INDEX IX_Donations_PantrySiteId ON Donations(PantrySiteId);
CREATE INDEX IX_Donations_ReceivedAt ON Donations(ReceivedAt);

CREATE TABLE InventoryLots (
    InventoryLotId INTEGER PRIMARY KEY AUTOINCREMENT,
    PantrySiteId INTEGER NOT NULL,
    FoodItemId INTEGER NOT NULL,
    DonationId INTEGER,
    QuantityReceived REAL NOT NULL CHECK (QuantityReceived BETWEEN 0 AND 100000),
    QuantityRemaining REAL NOT NULL CHECK (QuantityRemaining BETWEEN 0 AND QuantityReceived),
    ReceivedAt TEXT NOT NULL,
    ExpiresOn TEXT,
    FOREIGN KEY (PantrySiteId) REFERENCES PantrySites(PantrySiteId) ON DELETE RESTRICT,
    FOREIGN KEY (FoodItemId) REFERENCES FoodItems(FoodItemId) ON DELETE RESTRICT,
    FOREIGN KEY (DonationId) REFERENCES Donations(DonationId) ON DELETE RESTRICT
);
CREATE INDEX IX_InventoryLots_PantrySiteId_FoodItemId ON InventoryLots(PantrySiteId, FoodItemId);
CREATE INDEX IX_InventoryLots_ExpiresOn ON InventoryLots(ExpiresOn);
CREATE INDEX IX_InventoryLots_DonationId ON InventoryLots(DonationId);

CREATE TABLE Distributions (
    DistributionId INTEGER PRIMARY KEY AUTOINCREMENT,
    HouseholdId INTEGER NOT NULL,
    PantrySiteId INTEGER NOT NULL,
    DistributedAt TEXT NOT NULL,
    Notes TEXT CHECK (Notes IS NULL OR length(Notes) <= 300),
    FOREIGN KEY (HouseholdId) REFERENCES Households(HouseholdId) ON DELETE RESTRICT,
    FOREIGN KEY (PantrySiteId) REFERENCES PantrySites(PantrySiteId) ON DELETE RESTRICT
);
CREATE INDEX IX_Distributions_HouseholdId ON Distributions(HouseholdId);
CREATE INDEX IX_Distributions_PantrySiteId ON Distributions(PantrySiteId);
CREATE INDEX IX_Distributions_DistributedAt ON Distributions(DistributedAt);

CREATE TABLE DistributionLines (
    DistributionLineId INTEGER PRIMARY KEY AUTOINCREMENT,
    DistributionId INTEGER NOT NULL,
    FoodItemId INTEGER NOT NULL,
    InventoryLotId INTEGER NOT NULL,
    Quantity REAL NOT NULL CHECK (Quantity > 0 AND Quantity <= 100000),
    FOREIGN KEY (DistributionId) REFERENCES Distributions(DistributionId) ON DELETE CASCADE,
    FOREIGN KEY (FoodItemId) REFERENCES FoodItems(FoodItemId) ON DELETE RESTRICT,
    FOREIGN KEY (InventoryLotId) REFERENCES InventoryLots(InventoryLotId) ON DELETE RESTRICT
);
CREATE INDEX IX_DistributionLines_DistributionId ON DistributionLines(DistributionId);
CREATE INDEX IX_DistributionLines_FoodItemId ON DistributionLines(FoodItemId);
CREATE INDEX IX_DistributionLines_InventoryLotId ON DistributionLines(InventoryLotId);

CREATE TABLE Volunteers (
    VolunteerId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 1 AND 120),
    Email TEXT NOT NULL UNIQUE CHECK (length(trim(Email)) BETWEEN 1 AND 160),
    Phone TEXT CHECK (Phone IS NULL OR length(Phone) <= 30)
);

CREATE TABLE VolunteerShifts (
    VolunteerShiftId INTEGER PRIMARY KEY AUTOINCREMENT,
    VolunteerId INTEGER NOT NULL,
    PantrySiteId INTEGER NOT NULL,
    StartsAt TEXT NOT NULL,
    EndsAt TEXT NOT NULL,
    Role TEXT NOT NULL CHECK (length(trim(Role)) BETWEEN 1 AND 80),
    CHECK (EndsAt > StartsAt),
    FOREIGN KEY (VolunteerId) REFERENCES Volunteers(VolunteerId) ON DELETE CASCADE,
    FOREIGN KEY (PantrySiteId) REFERENCES PantrySites(PantrySiteId) ON DELETE RESTRICT
);
CREATE INDEX IX_VolunteerShifts_VolunteerId ON VolunteerShifts(VolunteerId);
CREATE INDEX IX_VolunteerShifts_PantrySiteId_StartsAt ON VolunteerShifts(PantrySiteId, StartsAt);
