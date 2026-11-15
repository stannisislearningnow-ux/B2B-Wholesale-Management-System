using B2BWholesale.AgentPortal.Models;
using B2BWholesale.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        B2BWholesaleDbContext db = scope.ServiceProvider.GetRequiredService<B2BWholesaleDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Items.AnyAsync()) return;

        Item[] items =
        {
            new() { ItemName = "Premium Jasmine Rice", Size = "5 kg bag", Price = 125000m, StockQuantity = 180 },
            new() { ItemName = "White Sugar", Size = "1 kg bag", Price = 26000m, StockQuantity = 240 },
            new() { ItemName = "Cooking Oil", Size = "2 litre bottle", Price = 78000m, StockQuantity = 120 },
            new() { ItemName = "Fish Sauce", Size = "750 ml bottle", Price = 42000m, StockQuantity = 150 },
            new() { ItemName = "Instant Noodles", Size = "30-pack carton", Price = 105000m, StockQuantity = 90 },
            new() { ItemName = "Green Tea", Size = "25 tea bags", Price = 55000m, StockQuantity = 100 },
            new() { ItemName = "Condensed Milk", Size = "380 g can", Price = 32000m, StockQuantity = 200 },
            new() { ItemName = "Ground Coffee", Size = "500 g bag", Price = 89000m, StockQuantity = 75 },
            new() { ItemName = "Dishwashing Liquid", Size = "1 litre bottle", Price = 47000m, StockQuantity = 110 },
            new() { ItemName = "Laundry Detergent", Size = "3 kg bag", Price = 135000m, StockQuantity = 65 },
            new() { ItemName = "Paper Towels", Size = "6-roll pack", Price = 68000m, StockQuantity = 95 },
            new() { ItemName = "Mineral Water", Size = "24-bottle carton", Price = 72000m, StockQuantity = 160 },
            new() { ItemName = "Canned Tuna", Size = "170 g can", Price = 29000m, StockQuantity = 130 },
            new() { ItemName = "Soy Sauce", Size = "500 ml bottle", Price = 35000m, StockQuantity = 140 },
            new() { ItemName = "Chocolate Biscuits", Size = "12-pack carton", Price = 96000m, StockQuantity = 8 }
        };

        string[] agentNames =
        {
            "An Phu Trading", "Binh Minh Distribution", "Central Market Supplies", "Dai Viet Wholesale",
            "East Star Trading", "Gia Han Retail Group", "Hung Thinh Supplies", "Kim Long Commerce",
            "Lan Anh Stores", "Mekong Retail Partners", "Nam Phuong Distribution", "Phuoc Loc Trading",
            "Queen Mart Group", "Song Xanh Wholesale", "Thanh Cong Stores"
        };
        Agent[] agents = agentNames.Select((name, index) => new Agent
        {
            AgentName = name,
            Address = $"{10 + index} Nguyen Trai, Ho Chi Minh City",
            Phone = $"090100{index + 1:0000}"
        }).ToArray();

        List<AppUser> users = new();
        users.Add(new AppUser { UserName = "admin01", Email = "admin01@wholesale.local", PasswordHash = "SETUP_REQUIRED", Role = "Admin" });
        users.Add(new AppUser { UserName = "admin02", Email = "admin02@wholesale.local", PasswordHash = "SETUP_REQUIRED", Role = "Admin" });
        users.Add(new AppUser { UserName = "manager", Email = "manager@wholesale.local", PasswordHash = "SETUP_REQUIRED", Role = "Admin" });
        for (int i = 0; i < 12; i++)
        {
            users.Add(new AppUser
            {
                UserName = $"agent{i + 1:00}",
                Email = $"agent{i + 1:00}@wholesale.local",
                PasswordHash = PasswordHasher.HashPassword("AgentDemo123!"),
                Role = "Agent",
                Agent = agents[i]
            });
        }

        WholesaleOrder[] orders = Enumerable.Range(1, 15).Select(i => new WholesaleOrder
        {
            OrderDate = new DateTime(2026, 10, i, 9, 0, 0, DateTimeKind.Utc),
            Agent = agents[i - 1],
            OrderStatus = i == 15 ? "Cancelled" : i % 3 == 0 ? "Confirmed" : i % 2 == 0 ? "Pending" : "Shipped"
        }).ToArray();

        int[,] pairs =
        {
            {1,3},{2,5},{4,7},{8,9},{10,11},{12,13},{14,15},{1,2},{3,4},{5,6},
            {7,8},{9,10},{11,12},{13,14},{15,1}
        };
        for (int i = 0; i < orders.Length; i++)
        {
            int itemA = pairs[i, 0] - 1;
            int itemB = pairs[i, 1] - 1;
            orders[i].OrderDetails.Add(new OrderDetail
            {
                Item = items[itemA], Quantity = 4 + i % 7, UnitAmount = items[itemA].Price
            });
            orders[i].OrderDetails.Add(new OrderDetail
            {
                Item = items[itemB], Quantity = 3 + i % 5, UnitAmount = items[itemB].Price
            });
        }

        db.Items.AddRange(items);
        db.Agents.AddRange(agents);
        db.Users.AddRange(users);
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }
}
