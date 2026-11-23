# Developer Guide

## Stack

- ASP.NET Core MVC on .NET 8
- Entity Framework Core 8 with SQLite
- Bootstrap, local static assets, and Razor views
- Open-Meteo forecast endpoint through `IHttpClientFactory`

## Setup

1. Install the .NET 8 SDK.
2. From the repository root, run `dotnet restore`.
3. Run `dotnet build`.
4. Start with `dotnet run --project .\FoodPantryManagement\FoodPantryManagement.csproj`.

The application database is `FoodPantryManagement/foodpantry.db`, created with `EnsureCreated` and seeded on first run. For a fresh application demo, stop the app and back up/remove that specific database file; the app will recreate it. Do not use `EnsureCreated` as a production migration strategy.

## Design and integrity

Models define required fields and ranges; `PantryDbContext` defines foreign-key delete behavior and indexes. Donation intake and distribution changes use database transactions. A distribution line records its inventory lot, so edit/delete restores quantities to the correct source. Stock is consumed FEFO (first-expiring, first-out).

Use EF-backed validation and explicit error messages. Preserve historical references: referenced donors, households, food items, sites, and stock lots are not silently removed.

## SQL and database exercises

The `database/` directory contains SQLite DDL, sample inserts, transactional update/delete examples, and join/aggregate reports. Scripts are for a separate demo database. The application seeds its own database through `PantrySeed`.

## GitHub team process

- Create an Issue for each bounded task and attach it to the active milestone.
- Branch from the agreed default branch using a descriptive name.
- Keep each pull request focused; explain testing and schema changes.
- Request review from a teammate, address comments, and merge only after CI/build checks pass.
- Never commit real pantry client data or credentials.

## Current limitations and next work

There is no authentication/authorization, configurable multi-tenant support, dedicated audit log, automated test project, or schema migration history. Add those before any real-world deployment. A future release should add volunteer and pantry-site management screens, backup/restore procedures, and explicit privacy/retention controls.
