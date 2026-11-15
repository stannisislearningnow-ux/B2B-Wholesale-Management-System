# B2B Wholesale Management System

Software Engineering Lab Assignment 2 - dual-platform solution.

## Included projects

| Project | Technology | Assignment tasks |
|---|---|---|
| `B2BWholesale` | Windows Forms, .NET Framework 4.7.2 | Admin login, Item and Agent CRUD, order status, shipping manifest, dashboard |
| `B2BWholesale.AgentPortal` | ASP.NET Core MVC, .NET 8, EF Core 8 | Agent login, catalog, transactional order placement, My Orders, printable invoice, responsive UI |
| `B2BWholesale.Core` | .NET Standard 2.0 | Shared PBKDF2 password hashing and business rules |
| `B2BWholesale.Tests` | MSTest, .NET 8 | Seven unit tests for stock, order calculations, duplicate order lines, lockout and password hashing |

## Requirements

- Visual Studio 2022 17.8 or later with **.NET desktop development** and **ASP.NET and web development** workloads.
- .NET Framework 4.7.2 Developer Pack.
- .NET 8 SDK for the ASP.NET Core and MSTest projects.
- SQL Server Express (or another SQL Server instance) and SQL Server Management Studio.

The web project targets .NET 8 so it can be opened with Visual Studio 2022 17.8 or later. .NET 8 support ended on 11 November 2026; this target is retained for compatibility with Visual Studio 2022. For a newly deployed production system, retarget the web and test projects to a currently supported .NET release and update the corresponding EF Core packages.

## Database setup

### New database

1. Start SQL Server Express. The default configured instance is `SQLEXPRESS`.
2. Open SSMS and connect to `\\.\SQLEXPRESS` using Windows Authentication.
3. Open and execute `data/B2BWholesaleDB.sql` once on a clean database.
4. If you use another SQL Server instance, change the `B2BWholesaleDB` connection string in `B2BWholesale/App.config` and `B2BWholesale.AgentPortal/appsettings.json`.

## Demo accounts

- **Agent Portal:** `agent01` / `AgentDemo123!` (the SQL seed creates separate salted PBKDF2-SHA256 hashes for the sample Agent accounts).
- **WinForms Admin:** run the desktop app once and use **Set initial Admin password** to configure `admin01` (or another Admin row), then sign in with the password you selected.

The sample Admin hash is deliberately a setup placeholder. Do not use demo passwords in a real deployment.

## Open and run in Visual Studio

1. Open the root `B2BWholesale.sln`.
2. Restore NuGet packages and build the solution.
3. Execute the database script in SSMS before starting either application.
4. To run the desktop back office, set `B2BWholesale` as the startup project and press **F5**.
5. To run the Agent Portal, set `B2BWholesale.AgentPortal` as the startup project and press **F5**. The launch profile uses `https://localhost:7180` and `http://localhost:5180`.
6. Run the tests from **Test > Run All Tests** or Test Explorer. The tests are in `B2BWholesale.Tests`.

Both applications use the same `B2BWholesaleDB` database.

## Main implementation details

- Passwords are stored as PBKDF2-HMAC-SHA256 hashes with a per-password random salt.
- Admin and Agent accounts are separated by role. The Agent Portal controllers require the `Agent` role.
- Five consecutive invalid passwords lock an account. An administrator can unlock a user by setting `[Lock] = 0` and `FailedLoginAttempts = 0` in the Users table.
- The order service uses a serializable SQL transaction. It creates the order master, creates all detail rows, and deducts stock inside the same transaction. Conditional stock updates prevent negative stock; any failure rolls the transaction back.
- Duplicate item lines are combined before stock is checked.
- The invoice action filters by both the requested OrderID and the signed-in AgentID, so an Agent cannot open another Agent's invoice.
- Order status values are `Pending`, `Confirmed`, `Shipped`, and `Cancelled`. The desktop dropdown and database check constraint use the same values.
- The WinForms dashboard shows best-selling items, low-stock items (default threshold 10), and revenue by Agent. Cancelled orders are excluded from revenue.

## Configuration

WinForms reads its connection from `B2BWholesale/App.config` using `ConfigurationManager.ConnectionStrings["B2BWholesaleDB"]`. The Agent Portal reads `ConnectionStrings:B2BWholesaleDB` from `B2BWholesale.AgentPortal/appsettings.json`.

The supplied examples use Windows Authentication and contain no SQL username or password. For SQL Authentication, keep the password outside source control and supply it using the `ConnectionStrings__B2BWholesaleDB` environment variable or user-secrets. Do not commit real credentials.

## Git and final submission checklist

Create a GitHub repository and make feature branches and reviewed pull requests for the actual team members. This ZIP does not invent a repository URL, commit history, PRs, screenshots, student contribution percentages, or report evidence. Add those real items to the supplied workbook and report template before submission.

The assignment brief specifies **3 to 5 students per group**. Confirm the group size with the instructor if your actual team has fewer than three members. The official submission also requires a completed `Report.docx`, a working team workbook, and a `Readme.txt` with the real GitHub link, contribution percentages and DB instructions.

See `DatabaseDiagram.mmd` for the ER diagram source.
