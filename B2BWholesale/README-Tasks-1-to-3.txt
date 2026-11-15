B2B WHOLESALE - WINFORMS TASKS 1.1 TO 1.3

Project target:
- .NET Framework 4.7.2
- SQL Server database: B2BWholesaleDB
- Connection string name: B2BWholesaleDB

WHAT IS INCLUDED
1. Task 1.1 - Admin-only login
   - Application starts at LoginForm.
   - Only an unlocked account with Role = 'Admin' can log in.
   - Passwords are stored as PBKDF2-SHA256 hashes with random salts.
   - Agent accounts are denied access.
   - First-run password setup is shown only while no Admin password hash
     has been configured.

2. Task 1.2 - Back-office management
   - Items: view, add, edit, delete, and manage stock quantity.
   - Agents: view, add, edit, delete.
   - Incoming web orders: view and update OrderStatus.

3. Task 1.3 - Shipping Manifest
   - Select an order and click Shipping Manifest.
   - A print preview is displayed.
   - Choose Yes after preview, select Microsoft Print to PDF, and save
     the generated manifest as a PDF file.

FIRST RUN
1. Open B2BWholesale.slnx or B2BWholesale.csproj in Visual Studio.
2. Check App.config and confirm the SQL Server instance in the connection
   string. The supplied value is .\\SQLEXPRESS.
3. Confirm the B2BWholesaleDB database and required tables exist.
4. Build the project.
5. Run the application.
6. On the first run, click "Set initial Admin password".
7. Choose an Admin username, enter a password of at least 8 characters,
   confirm it, and save.
8. Return to login and sign in using that Admin username and password.

DATABASE TABLE/COLUMN EXPECTATIONS
Item: ItemID, ItemName, Size, Price, StockQuantity
Agent: AgentID, AgentName, Address, Phone
Order: OrderID, OrderDate, AgentID, OrderStatus
OrderDetail: ID, OrderID, ItemID, Quantity, UnitAmount
Users: UserID, UserName, PasswordHash, Lock, Role

IMPORTANT
- The database itself is not included in this ZIP. It must already exist
  in SQL Server.
- The project does not include the separate ASP.NET Agent Portal.
- These files implement WinForms Tasks 1.1, 1.2 and 1.3. They do not
  implement Task 1.4 (Dashboard/Data Filter).
- If your actual database column names differ from the list above, the
  SQL statements in data/Database.cs must be adjusted to match.
- First-run password setup is intended for initial local deployment.
  Once any Admin password has been configured, the setup option is hidden.
