using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace B2BWholesale.Data
{
    public class Database
    {
        private readonly string connectionString;

        public Database()
        {
            ConnectionStringSettings setting =
                ConfigurationManager.ConnectionStrings["B2BWholesaleDB"];
            if (setting == null || String.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new InvalidOperationException(
                    "Connection string 'B2BWholesaleDB' was not found in App.config.");
            connectionString = setting.ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public DataTable GetItems()
        {
            return Query("SELECT ItemID, ItemName, Size, Price, StockQuantity FROM Item ORDER BY ItemID");
        }

        public DataTable GetAgents()
        {
            return Query("SELECT AgentID, AgentName, Address, Phone FROM Agent ORDER BY AgentID");
        }

        public DataTable GetOrders()
        {
            return Query(@"SELECT o.OrderID, o.OrderDate, o.AgentID, a.AgentName,
                                  o.OrderStatus,
                                  ISNULL(SUM(od.Quantity * od.UnitAmount), 0) AS OrderTotal
                           FROM [Order] o
                           INNER JOIN Agent a ON a.AgentID = o.AgentID
                           LEFT JOIN OrderDetail od ON od.OrderID = o.OrderID
                           GROUP BY o.OrderID, o.OrderDate, o.AgentID, a.AgentName, o.OrderStatus
                           ORDER BY o.OrderDate DESC, o.OrderID DESC");
        }

        public DataTable GetOrderManifest(int orderId)
        {
            return Query(@"SELECT o.OrderID, o.OrderDate, o.OrderStatus,
                                  a.AgentName, a.Address, a.Phone,
                                  i.ItemID, i.ItemName, i.Size,
                                  od.Quantity, od.UnitAmount,
                                  (od.Quantity * od.UnitAmount) AS LineTotal
                           FROM [Order] o
                           INNER JOIN Agent a ON a.AgentID = o.AgentID
                           INNER JOIN OrderDetail od ON od.OrderID = o.OrderID
                           INNER JOIN Item i ON i.ItemID = od.ItemID
                           WHERE o.OrderID = @OrderID
                           ORDER BY i.ItemName",
                new SqlParameter("@OrderID", orderId));
        }

        public DataTable GetAdminUsers()
        {
            return Query(@"SELECT UserID, UserName FROM Users
                           WHERE Role = 'Admin' AND ISNULL([Lock], 0) = 0
                           ORDER BY UserName");
        }

        public bool HasConfiguredAdminPassword()
        {
            object result = Scalar(@"SELECT COUNT(*) FROM Users
                WHERE Role = 'Admin' AND PasswordHash LIKE 'PBKDF2$SHA256$%'");
            return Convert.ToInt32(result) > 0;
        }

        public DataRow GetUserForLogin(string username)
        {
            DataTable t = Query(@"SELECT TOP 1 UserID, UserName, PasswordHash,
                                         ISNULL([Lock], 0) AS IsLocked,
                                         ISNULL(FailedLoginAttempts, 0) AS FailedLoginAttempts, Role
                                  FROM Users WHERE UserName = @UserName",
                new SqlParameter("@UserName", username));
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }

        public bool RegisterFailedLogin(int userId)
        {
            object result = Scalar(@"UPDATE Users
                SET FailedLoginAttempts = CASE WHEN FailedLoginAttempts < 5 THEN FailedLoginAttempts + 1 ELSE 5 END,
                    [Lock] = CASE WHEN FailedLoginAttempts + 1 >= 5 THEN 1 ELSE [Lock] END
                OUTPUT inserted.[Lock]
                WHERE UserID = @UserID AND ISNULL([Lock], 0) = 0",
                new SqlParameter("@UserID", userId));
            return result != null && Convert.ToBoolean(result);
        }

        public void ResetLoginFailures(int userId)
        {
            Execute(@"UPDATE Users SET FailedLoginAttempts = 0
                      WHERE UserID = @UserID AND ISNULL([Lock], 0) = 0",
                new SqlParameter("@UserID", userId));
        }

        public void SetInitialAdminPassword(int userId, string passwordHash)
        {
            int changed = Execute(@"UPDATE Users SET PasswordHash = @Hash, FailedLoginAttempts = 0
                WHERE UserID = @UserID AND Role = 'Admin'
                AND (PasswordHash IS NULL OR PasswordHash NOT LIKE 'PBKDF2$SHA256$%')",
                new SqlParameter("@Hash", passwordHash),
                new SqlParameter("@UserID", userId));
            if (changed != 1)
                throw new InvalidOperationException(
                    "The selected Admin account already has a configured password, or it could not be updated.");
        }

        public void AddItem(string name, string size, decimal price, int stock)
        {
            Execute(@"INSERT INTO Item (ItemName, Size, Price, StockQuantity)
                      VALUES (@Name, @Size, @Price, @Stock)",
                new SqlParameter("@Name", name), new SqlParameter("@Size", size),
                new SqlParameter("@Price", price), new SqlParameter("@Stock", stock));
        }

        public void UpdateItem(int id, string name, string size, decimal price, int stock)
        {
            Execute(@"UPDATE Item SET ItemName=@Name, Size=@Size, Price=@Price,
                      StockQuantity=@Stock WHERE ItemID=@ID",
                new SqlParameter("@Name", name), new SqlParameter("@Size", size),
                new SqlParameter("@Price", price), new SqlParameter("@Stock", stock),
                new SqlParameter("@ID", id));
        }

        public void DeleteItem(int id)
        {
            Execute("DELETE FROM Item WHERE ItemID=@ID", new SqlParameter("@ID", id));
        }

        public void AddAgent(string name, string address, string phone)
        {
            Execute(@"INSERT INTO Agent (AgentName, Address, Phone)
                      VALUES (@Name, @Address, @Phone)",
                new SqlParameter("@Name", name), new SqlParameter("@Address", address),
                new SqlParameter("@Phone", phone));
        }

        public void UpdateAgent(int id, string name, string address, string phone)
        {
            Execute(@"UPDATE Agent SET AgentName=@Name, Address=@Address, Phone=@Phone
                      WHERE AgentID=@ID",
                new SqlParameter("@Name", name), new SqlParameter("@Address", address),
                new SqlParameter("@Phone", phone), new SqlParameter("@ID", id));
        }

        public void DeleteAgent(int id)
        {
            Execute("DELETE FROM Agent WHERE AgentID=@ID", new SqlParameter("@ID", id));
        }

        public DataTable GetBestSellingItems()
        {
            return Query(@"SELECT TOP 10 i.ItemID, i.ItemName, i.Size,
                                  SUM(od.Quantity) AS UnitsSold,
                                  SUM(od.Quantity * od.UnitAmount) AS SalesAmount
                           FROM Item i
                           INNER JOIN OrderDetail od ON od.ItemID = i.ItemID
                           INNER JOIN [Order] o ON o.OrderID = od.OrderID
                           WHERE o.OrderStatus <> 'Cancelled'
                           GROUP BY i.ItemID, i.ItemName, i.Size
                           ORDER BY UnitsSold DESC, i.ItemName");
        }

        public DataTable GetLowStockItems(int threshold)
        {
            return Query(@"SELECT ItemID, ItemName, Size, StockQuantity
                           FROM Item WHERE StockQuantity <= @Threshold
                           ORDER BY StockQuantity, ItemName",
                new SqlParameter("@Threshold", threshold));
        }

        public DataTable GetRevenueByAgent()
        {
            return Query(@"SELECT a.AgentID, a.AgentName,
                                  COUNT(DISTINCT o.OrderID) AS OrderCount,
                                  ISNULL(SUM(CASE WHEN o.OrderStatus <> 'Cancelled'
                                                  THEN od.Quantity * od.UnitAmount ELSE 0 END), 0) AS TotalRevenue
                           FROM Agent a
                           LEFT JOIN [Order] o ON o.AgentID = a.AgentID
                           LEFT JOIN OrderDetail od ON od.OrderID = o.OrderID
                           GROUP BY a.AgentID, a.AgentName
                           ORDER BY TotalRevenue DESC, a.AgentName");
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            string[] allowed = { "Pending", "Confirmed", "Shipped", "Cancelled" };
            if (Array.IndexOf(allowed, status) < 0)
                throw new ArgumentException("Invalid order status.", nameof(status));
            Execute("UPDATE [Order] SET OrderStatus=@Status WHERE OrderID=@ID",
                new SqlParameter("@Status", status),
                new SqlParameter("@ID", orderId));
        }
    }
}
