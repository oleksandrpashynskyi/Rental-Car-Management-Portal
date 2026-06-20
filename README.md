# Rental Car Management Portal

A C# **Windows Forms** desktop application for a car-rental company. It calculates rental pricing from the selected vehicle class and rental period, records each rental in a SQL Server database via ADO.NET, and lists existing rentals in a data grid.

## What it does

- **Choose a vehicle class** — Compact ($25/day), SUV ($40/day), or Sports Car ($60/day).
- **Pick rent and return dates** — the return date is constrained to be after the rent date.
- **Optional add-on** — an extra-driver charge ($25).
- **Place Rental** — computes the number of days, subtotal, tax, extra-driver fee, and total, then inserts a row into the `dbo.Rentals` table (license plate, car type, dates, and the computed amounts).
- **Fill** — loads existing rentals from the database into a read-only `DataGridView`.
- **Update** — reserved as a placeholder for a future editing feature.

On startup the app **provisions its own schema**: if the `dbo.Rentals` table doesn't exist it is created (and missing columns are added), so a fresh database works without manual setup beyond creating the database itself.

## Tech stack

- **Language:** C# (.NET Framework)
- **UI:** Windows Forms (programmatic, responsive `TableLayoutPanel`/`FlowLayoutPanel` layout)
- **Data access:** ADO.NET (`System.Data.SqlClient`)
- **Database:** Microsoft SQL Server LocalDB

## Project structure

```
Rental-Car-Management-Portal/
├── Program.cs                 # WinForms entry point
├── Form1.cs                   # UI construction, pricing logic, and all data access
├── Form1.Designer.cs          # designer-generated partial
├── App.config                 # "RentalsDb" connection string (LocalDB)
├── App_Data/SQLQuery1.sql     # small diagnostic query
├── RentalForCars.csproj
├── packages.config
└── Properties/                # assembly info, resources, settings
```

## Prerequisites

- Windows with .NET Framework
- Visual Studio (2019/2022) or MSBuild
- SQL Server **LocalDB** (`(LocalDB)\MSSQLLocalDB`)

## Setup and run

1. **Create the database** once (the app creates the *table*, but not the database):

   ```sql
   -- e.g. via sqlcmd against (localdb)\MSSQLLocalDB
   CREATE DATABASE RentalsDb;
   ```

   The connection string in `App.config` (`name="RentalsDb"`) targets `(LocalDB)\MSSQLLocalDB` with `Initial Catalog=RentalsDb`. Adjust it if your LocalDB instance or database name differs.

2. **Open** `RentalForCars.csproj` in Visual Studio (or build with MSBuild) and **run**. On first launch the app ensures the `dbo.Rentals` table exists.

3. Place a rental, then click **Fill** to see it appear in the grid.

## Notes

- Pricing rules (daily rates, tax, extra-driver fee) are applied in application code in `Form1.cs`.
- The **Update** button is currently a placeholder; rental rows are inserted and listed, not edited in-place.
