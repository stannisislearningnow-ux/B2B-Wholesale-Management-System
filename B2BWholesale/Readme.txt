B2B WHOLESALE MANAGEMENT SYSTEM - SETUP AND SUBMISSION NOTES

1. GITHUB REPOSITORY
   [ADD THE REAL GITHUB REPOSITORY URL]

2. VIDEO DEMO (OPTIONAL)
   [ADD THE REAL VIDEO LINK OR WRITE N/A]

3. DATABASE CONFIGURATION
   - SQL Server instance in the provided settings: .\SQLEXPRESS
   - Database name: B2BWholesaleDB
   - New database: execute data\B2BWholesaleDB.sql once in SSMS.
   - Existing database from the earlier ZIP: execute data\Upgrade-Existing-Database.sql once.
   - WinForms connection: B2BWholesale\App.config, key B2BWholesaleDB.
   - Web connection: B2BWholesale.AgentPortal\appsettings.json, key ConnectionStrings:B2BWholesaleDB.
   - For SQL Authentication, set ConnectionStrings__B2BWholesaleDB outside source control.

4. DEMO ACCOUNTS
   - Agent Portal: agent01 / AgentDemo123!
   - Admin: start B2BWholesale, use Set initial Admin password for admin01, then log in with the password you configured.

5. RUNNING THE SOLUTION
   - Open B2BWholesale.sln in Visual Studio 2022 17.8+.
   - Restore NuGet packages and build.
   - Run B2BWholesale for the desktop back office.
   - Run B2BWholesale.AgentPortal for the web portal (https://localhost:7180).
   - Run the B2BWholesale.Tests project in Test Explorer.

6. CONTRIBUTION
   [CONFIRM THE REAL STUDENT NAMES, IDS, TASKS AND CONTRIBUTION PERCENTAGES WITH THE GROUP]
   The total contribution must equal 100% and must match the report and team workbook.

7. SUBMISSION REMINDER
   Add the actual GitHub link, screenshots, database diagram image, report, and team evidence before submitting. Do not claim that code implementation alone proves completed Git branching or pull requests.
