using B2BWholesale.AgentPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Data;

public sealed class B2BWholesaleDbContext : DbContext
{
    public B2BWholesaleDbContext(DbContextOptions<B2BWholesaleDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<WholesaleOrder> Orders => Set<WholesaleOrder>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item", table =>
            {
                table.HasCheckConstraint("CK_Item_Price", "[Price] >= 0");
                table.HasCheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
            });
            entity.HasKey(x => x.ItemID);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Size).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Price).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agent");
            entity.HasKey(x => x.AgentID);
            entity.Property(x => x.AgentName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.HasIndex(x => x.Phone).IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users", table =>
            {
                table.HasCheckConstraint("CK_Users_FailedLoginAttempts", "[FailedLoginAttempts] >= 0");
                table.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin', 'Agent')");
                table.HasCheckConstraint("CK_Users_AgentRole", "([Role] = 'Admin' AND [AgentID] IS NULL) OR ([Role] = 'Agent' AND [AgentID] IS NOT NULL)");
            });
            entity.HasKey(x => x.UserID);
            entity.Property(x => x.UserName).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Lock).HasColumnName("Lock").HasDefaultValue(false);
            entity.Property(x => x.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.AgentID).IsUnique().HasFilter("[AgentID] IS NOT NULL");
            entity.HasOne(x => x.Agent)
                .WithOne(x => x.User)
                .HasForeignKey<AppUser>(x => x.AgentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WholesaleOrder>(entity =>
        {
            entity.ToTable("Order", table =>
                table.HasCheckConstraint("CK_Order_Status", "[OrderStatus] IN ('Pending', 'Confirmed', 'Shipped', 'Cancelled')"));
            entity.HasKey(x => x.OrderID);
            entity.Property(x => x.OrderDate).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
            entity.Property(x => x.OrderStatus).HasMaxLength(20).HasDefaultValue("Pending").IsRequired();
            entity.HasIndex(x => x.AgentID);
            entity.HasIndex(x => x.OrderDate);
            entity.HasOne(x => x.Agent)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AgentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("OrderDetail", table =>
            {
                table.HasCheckConstraint("CK_OrderDetail_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderDetail_UnitAmount", "[UnitAmount] >= 0");
            });
            entity.HasKey(x => x.ID);
            entity.Property(x => x.UnitAmount).HasPrecision(12, 2);
            entity.HasIndex(x => new { x.OrderID, x.ItemID }).IsUnique();
            entity.HasIndex(x => x.ItemID);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.OrderID)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Item)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.ItemID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
