# Meeting Notes

## Team roles

- **Tiyana Harden:** planning, coordination, and validation.
- **Isa:** C# application.
- **Terrell Taylor (@TerrellTaylor):** SQL and database work.
- **Dontrez:** project structure.

## Team-reported timeline

These entries were written up after the project from the dates and details the team provided. Everyone attended the listed check-ins. They are summaries, not verbatim minutes.

### October 13, 2026 — Project kickoff

- The team began planning the project and discussed the food pantry idea.

### October 16, 2026 — Project selected and work divided

- Everyone attended and chose the Food Pantry project.
- The team divided the work: Tiyana handled planning, coordination, and validation; Isa handled the C# application; Terrell handled SQL; and Dontrez handled the project structure.

### October 22, 2026 — Team check-in

- Everyone attended and discussed the work each person was handling. More detailed decisions or action items were not provided for this date.

### October 27, 2026 — Team check-in

- Everyone attended and discussed the work in progress. More detailed decisions or action items were not provided for this date.

### November 3, 2026 — Team check-in

- Everyone attended and discussed the project work. More detailed decisions or action items were not provided for this date.

## Challenges and fixes

### Database design

- One design challenge was keeping donated quantities and current stock consistent without storing the same quantity twice. The final schema records each donated batch as an `InventoryLot` linked to its `Donation`; distribution lines refer to the exact lot used.
- Primary and foreign keys keep those records connected. The schema and SQL scripts were checked with foreign-key enforcement enabled.

### Keeping inventory accurate

- Donations and distributions change stock, and editing or deleting a distribution must put quantities back into the correct lots.
- Donation and distribution saves use transactions. Distributions take stock from the earliest-expiring lots first, reject requests that exceed available stock, and restore the old quantities before an edit or delete. A stock change during save is rolled back and shown to the user instead of leaving a partial update.
- Validation also prevents negative quantities, duplicate item entries, and invalid donor, household, or pantry references.

### Route testing

- The specific HTTP error reported during testing was `404` for `/api/customers`. Pantry Table is an ASP.NET Core MVC app and does not define that Web API endpoint, so the URL did not match a route in this project.
- We checked the app's MVC routing and tested the existing `/Inventory` page at `http://127.0.0.1:5094/Inventory`; it returned HTTP `200`. That is a successful response, not an error.

### Weather service reliability

- The weather forecast depends on an external service, so a timeout, network problem, malformed JSON, or incomplete response should not stop pantry work. The client uses an eight-second timeout, logs these cases, and shows a weather-unavailable message while keeping pantry records available.
- The project notes do not record a separate weather-provider failure; this handling is in place for reliability.

### Database setup note

- The demo database is created with EF Core `EnsureCreated`, which does not apply schema migrations to an existing database. After a structural schema change, use a fresh demo database before validating the new design.

## Completion

- The project was reported complete on November 22, 2026. The app, SQL scripts, and documentation were pushed to the GitHub repository that day.
- The team plans to keep work visible in GitHub Issues, milestones, branches, and pull requests. A teammate must submit any real code review; this document does not claim one has happened.
