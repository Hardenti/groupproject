# Pantry Table

Pantry Table is a small, database-backed food pantry operations app. It helps pantry staff track donated food, lot expiration dates, household registrations, and food distributions without losing stock history. A weather forecast gives pickup coordinators a practical heads-up when rain or severe weather may affect outdoor service.

## What it does

- Manage a normalized food catalog, donors, households, and inventory lots.
- Record multi-item donations and add each item to stock automatically.
- Search and sort pantry records.
- Prepare, edit, and cancel household distributions. Stock is deducted from the earliest-expiring lots first and returned if a distribution is canceled.
- Validate form values and prevent over-distribution and unsafe deletes.
- Show low-stock and near-expiry alerts on the dashboard.
- Retrieve a seven-day forecast for the pantry site using Open-Meteo.

## Run locally

Requirements: .NET 8 SDK and an internet connection for the optional weather forecast.

```powershell
dotnet run --project .\FoodPantryManagement\FoodPantryManagement.csproj
```

Open the localhost URL printed by ASP.NET Core. The app creates `FoodPantryManagement\foodpantry.db` and sample records on first launch. The sample pantry location is Springfield, Illinois; update the `PantrySites` row in the database with your pantry's actual address and coordinates for a useful local forecast.

> This classroom prototype has no authentication or role-based access. Do not enter real clients' sensitive personal information or deploy it to a public network without adding security, privacy, backups, and operational review.

## Project map

| Path | Contents |
|---|---|
| `FoodPantryManagement/` | ASP.NET Core MVC application, EF Core models, services, and views |
| `database/schema.sql` | SQLite schema for the 11-table relational model |
| `database/sample_data.sql` | Demonstration records |
| `database/update_examples.sql` / `delete_examples.sql` | Safe, transactional data-change examples |
| `database/reports.sql` | Join, search, aggregate, and stock-report queries |
| `docs/` | Proposal, team planning, ERD, schema, API, wireframe, user/developer guides, and reflection |

## Project documentation

- [Project Proposal](docs/Project-Proposal.md)
- [Meeting Notes](docs/Meeting-Notes.md)
- [Sprint Plans](docs/Sprint-Plans.md)
- [ER Diagram](docs/ER-Diagram.md)
- [Database Schema](docs/Database-Schema.md)
- [SQL Scripts](database/)
- [Wireframes](docs/Wireframes.md)
- [API Documentation](docs/API-Documentation.md)
- [User Guide](docs/User-Guide.md)
- [Developer Guide](docs/Developer-Guide.md)
- [Final Reflection](docs/Final-Reflection.md)

## Database and sample data

The app uses SQLite with EF Core `EnsureCreated` for a no-migration classroom setup. To inspect the standalone SQL scripts with the SQLite CLI:

```powershell
sqlite3 pantry-demo.db ".read database/schema.sql"
sqlite3 pantry-demo.db ".read database/sample_data.sql"
```

The app database and standalone demonstration database are separate files. The script's sample data is not required for the app, which seeds its own sample records on first run.

## Team workflow

Use GitHub Issues for tasks, a milestone for each sprint, feature branches for work, and pull requests with a review from another team member before merging. Record real decisions and contributions in the meeting notes and reflection; do not treat the starter templates as evidence that meetings or reviews occurred.
