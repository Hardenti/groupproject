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

## Testing issue

- During route testing, `/api/customers` returned HTTP 404. Pantry Table is an ASP.NET Core MVC application and does not define that API endpoint, so that URL was not a valid route for this project.
- We checked the running app at its configured local address and used the existing MVC route `/Inventory`. That page returned HTTP 200, which means the request succeeded; HTTP 200 is not an error.

## Completion

- The project was reported complete on November 22, 2026. The app, SQL scripts, and documentation were pushed to the GitHub repository that day.
- The team plans to keep work visible in GitHub Issues, milestones, branches, and pull requests. A teammate must submit any real code review; this document does not claim one has happened.
