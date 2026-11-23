# User Guide

## Start the application

Run the app from a terminal with `dotnet run --project .\FoodPantryManagement\FoodPantryManagement.csproj`, then open the local address printed in the terminal. The first run creates a demo pantry and sample records.

## Receive a donation

1. Choose **Donations** → **Receive donation**.
2. Select a donor and the receiving pantry.
3. Enter the received date, quantities, and optional best-before dates for donated items.
4. Save. The receipt and its inventory lots are created together.
5. Use the donation details or inventory page to verify the result.

Add a new donor or food item using the links on the relevant page. A pantry administrator can deactivate food items no longer accepted.

## Manage stock

Use **Inventory** to search and sort lots by expiry, item, or quantity. Choose **Adjust** to correct the remaining balance or expiry date. The app will not allow reducing a lot below the total quantity already distributed from that lot.

## Register households

Choose **Households** → **Register household**. Enter the primary contact, household size, and optional contact details. Search the list by name, email, or phone; sort by household size or registration date.

## Prepare a distribution

1. Choose **Distributions** → **Prepare distribution**.
2. Select the household and pantry site.
3. Enter the quantity of each item to issue. The shown available amount is current stock at that site.
4. Save. The application rejects negative or excess quantities and consumes lots with the nearest expiry first.
5. Edit a distribution to reconcile quantities, or delete it to return the recorded quantities to their original lots.

## Dashboard and weather

The dashboard summarizes active catalog items, households, low stock, and lots expiring in seven days. The recent activity list shows completed distributions. The forecast advisory helps plan outdoor pickups; it is not a food safety or emergency warning.

## Demo records and privacy

The seeded records are examples. This prototype does not have user accounts, access control, encryption-at-rest configuration, or production backup/retention policy. Do not use real household personal information on a shared or public computer.
