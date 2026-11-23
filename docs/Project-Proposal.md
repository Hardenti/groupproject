# Pantry Table Project Proposal

## What we want to solve

Food pantries have to keep track of what comes in, what is still on the shelves, and what goes out to families. If those records are split between paper and spreadsheets, it is easy to miss low stock or food that is close to expiring.

Our project, Pantry Table, is a small web app for keeping those tasks together. Staff can record a donation, check inventory, register households, and record food distributions.

## Who would use it

- Pantry staff who receive donations and keep the catalog up to date.
- Volunteers who help prepare distributions.
- Coordinators who need to check stock and plan pickup days.

## What we plan to include

The app has pages for food items, donors, households, inventory lots, donations, and distributions. Users can add, view, change, and remove records where it is safe to do so. They can search and sort lists, and the app checks that required information is entered and that distributions do not exceed available stock.

The dashboard also checks the Open-Meteo forecast for the pantry location. If rain or severe weather is likely, it shows a reminder that may help staff plan an outdoor pickup.

## Database

We use SQLite and Entity Framework Core. The database has 11 related tables with primary keys and foreign keys. The ER diagram and SQL scripts are included in this project.

## What is not included

This version does not have user accounts, roles, payment processing, or production backup and privacy controls. It uses sample data only. It should not be used to store real household information or run a real pantry without further security work.

## How we will know it works

1. Recording a donation adds the donated quantities to inventory.
2. A distribution cannot issue more food than the pantry has.
3. Editing or deleting a distribution puts the right quantities back in the right stock lots.
4. Staff can find low stock and food that is nearing its expiry date.
5. The weather forecast gives a useful pickup-planning reminder.

## Team

| Member | Main responsibility |
|---|---|
| Terrel | Database design, SQL scripts, and checking relationships |
| Tiyana Harden | C# application and connecting the pages to the database |
| Isa | Page layout, wireframes, and user-facing documentation |
| Dontrez | Weather API and testing the main workflows |

These are our starting roles. We can help each other and adjust the work as needed. Our project start date is October 13, 2026.
