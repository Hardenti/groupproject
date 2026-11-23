# Database Schema and 3NF Notes

Pantry Table uses SQLite and EF Core. The standalone creation script is [`database/schema.sql`](../database/schema.sql). The application uses the same table/entity concepts and creates its local database on first run.

## Tables

| Table | Purpose | Important foreign keys |
|---|---|---|
| `PantrySites` | Pantry location and forecast coordinates | — |
| `FoodCategories` | Reusable classification and storage requirement | — |
| `FoodItems` | Catalog item and unit of measure | `FoodCategoryId` |
| `Donors` | Donor contact record | — |
| `Donations` | Donation receipt header | `DonorId`, `PantrySiteId` |
| `InventoryLots` | Donated or manually received stock by item, site, and expiry | `PantrySiteId`, `FoodItemId`, optional `DonationId` |
| `Households` | Household contact and size | — |
| `Distributions` | One household pickup/issue event | `HouseholdId`, `PantrySiteId` |
| `DistributionLines` | Quantity issued and exact source lot | `DistributionId`, `FoodItemId`, `InventoryLotId` |
| `Volunteers` | Volunteer contact record | — |
| `VolunteerShifts` | Scheduled volunteer role and time | `VolunteerId`, `PantrySiteId` |

## Keys and integrity

Every table has an integer primary key. Foreign keys connect dependent records to their owners. Restrict rules protect historical donor, household, item, site, and inventory-lot references from accidental deletion; distribution line rows cascade with their header. Unique indexes protect category names and volunteer email addresses. Quantity and household-size inputs are validated in the application and checked in the SQL schema.

## 3NF rationale

- Each table describes one entity or one associative/detail record.
- Every row is identified by its primary key; contact, catalog, and location attributes are stored with their own entity rather than copied onto transaction lines.
- Non-key attributes depend on the key, the whole key, and not another non-key attribute. A food category's refrigeration requirement is stored once in `FoodCategories`, while each food item stores only its category foreign key.
- Each `InventoryLots` row represents one item/expiry batch, optionally tied to a `Donations` receipt. The donation header stores donor, site, and receipt date once; the lot stores item and quantity facts once.
- Each distribution detail row records its issued amount and exact lot; current remaining stock is maintained transactionally by the application.

`InventoryLots.QuantityRemaining` is an operational balance updated with each distribution. The original amount and issue history are retained for accountability; this value is intentionally maintained as a transactionally reconciled balance rather than derived for every page request.

## Indexes and delete policy

The schema creates indexes for lookup foreign keys, expiry reporting, and transaction dates. Do not delete referenced records to erase history; mark food items inactive or correct a transaction through the application.

## Diagram

See [ER Diagram](ER-Diagram.md) for entities and cardinalities.
