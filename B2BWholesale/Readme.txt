B2B WHOLESALE MANAGEMENT SYSTEM - SETUP AND SUBMISSION NOTES

1. GITHUB REPOSITORY
   https://github.com/stannisislearningnow-ux/B2B-Wholesale-Management-System

2. DATABASE CONFIGURATION
   - SQL Server instance in the provided settings: .\SQLEXPRESS
   - Database name: B2BWholesaleDB
   - New database: execute data\B2BWholesaleDB.sql once in SSMS.
   - Existing database from the earlier ZIP: execute data\Upgrade-Existing-Database.sql once.
   - WinForms connection: B2BWholesale\App.config, key B2BWholesaleDB.
   - Web connection: B2BWholesale.AgentPortal\appsettings.json, key ConnectionStrings:B2BWholesaleDB.
   - For SQL Authentication, set ConnectionStrings__B2BWholesaleDB outside source control.

3. DEMO ACCOUNTS
   - Agent Portal: agent01 / AgentDemo123!
   - Admin: start B2BWholesale, use Set initial Admin password for admin01, then log in with the password you configured.

4. RUNNING THE SOLUTION
   - Open B2BWholesale.sln in Visual Studio 2022 17.8+.
   - Restore NuGet packages and build.
   - Run B2BWholesale for the desktop back office.
   - Run B2BWholesale.AgentPortal for the web portal (https://localhost:7180).
   - Run the B2BWholesale.Tests project in Test Explorer.

5. CONTRIBUTION
   Ngô Xuân Thịnh - 524K0029: 50%
   Huỳnh Kỳ Anh - 524K0013: 50%
