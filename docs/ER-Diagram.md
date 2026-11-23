# Entity-Relationship Diagram

```mermaid
erDiagram
    PantrySites ||--o{ Donations : receives
    PantrySites ||--o{ InventoryLots : stores
    PantrySites ||--o{ Distributions : fulfills
    PantrySites ||--o{ VolunteerShifts : hosts
    Donors ||--o{ Donations : gives
    Donations o|--o{ InventoryLots : records
    FoodCategories ||--o{ FoodItems : classifies
    FoodItems ||--o{ InventoryLots : stocked_as
    Households ||--o{ Distributions : receives
    Distributions ||--|{ DistributionLines : contains
    FoodItems ||--o{ DistributionLines : distributed_as
    InventoryLots ||--o{ DistributionLines : issued_from
    Volunteers ||--o{ VolunteerShifts : works

    PantrySites {
        int PantrySiteId PK
        string Name
        string Address
        string City
        string StateCode
        string PostalCode
        real Latitude
        real Longitude
    }
    Donors {
        int DonorId PK
        string Name
        string Email
        string Phone
    }
    Households {
        int HouseholdId PK
        string PrimaryContactName
        string Email
        string Phone
        int HouseholdSize
        string Notes
        datetime CreatedAt
    }
    FoodCategories {
        int FoodCategoryId PK
        string Name UK
        boolean RequiresRefrigeration
    }
    FoodItems {
        int FoodItemId PK
        string Name
        string Unit
        int FoodCategoryId FK
        boolean IsActive
    }
    Donations {
        int DonationId PK
        int DonorId FK
        int PantrySiteId FK
        datetime ReceivedAt
        string Notes
    }
    InventoryLots {
        int InventoryLotId PK
        int PantrySiteId FK
        int FoodItemId FK
        int DonationId FK
        real QuantityReceived
        real QuantityRemaining
        date ReceivedAt
        date ExpiresOn
    }
    Distributions {
        int DistributionId PK
        int HouseholdId FK
        int PantrySiteId FK
        datetime DistributedAt
        string Notes
    }
    DistributionLines {
        int DistributionLineId PK
        int DistributionId FK
        int FoodItemId FK
        int InventoryLotId FK
        real Quantity
    }
    Volunteers {
        int VolunteerId PK
        string Name
        string Email UK
        string Phone
    }
    VolunteerShifts {
        int VolunteerShiftId PK
        int VolunteerId FK
        int PantrySiteId FK
        datetime StartsAt
        datetime EndsAt
        string Role
    }
```

The diagram contains 11 tables. Each donated item/expiry batch is represented directly by an inventory lot linked to its donation header, avoiding duplicate donation-line and lot quantities. Distribution details resolve the distribution's one-to-many item relationship. `DistributionLines.InventoryLotId` preserves the exact source lot for each issued quantity, which supports reliable stock restoration and traceability.
