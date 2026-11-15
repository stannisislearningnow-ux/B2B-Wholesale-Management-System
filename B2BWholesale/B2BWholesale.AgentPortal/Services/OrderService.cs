using System.Data;
using B2BWholesale.AgentPortal.Data;
using B2BWholesale.AgentPortal.Models;
using B2BWholesale.Core;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Services;

public sealed class OrderService
{
    private readonly B2BWholesaleDbContext _db;

    public OrderService(B2BWholesaleDbContext db) => _db = db;

    public async Task<int> PlaceOrderAsync(int agentId, IEnumerable<OrderLine> requestedLines, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderLine> lines = OrderMath.MergeDuplicateItems(requestedLines);
        if (lines.Count == 0)
            throw new InvalidOperationException("Choose at least one item and enter a quantity greater than zero.");

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            bool agentExists = await _db.Agents.AnyAsync(x => x.AgentID == agentId, cancellationToken);
            if (!agentExists) throw new InvalidOperationException("The signed-in agent account is not linked to an Agent record.");

            WholesaleOrder order = new()
            {
                AgentID = agentId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending"
            };

            foreach (OrderLine line in lines)
            {
                Item? item = await _db.Items.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.ItemID == line.ItemId, cancellationToken);
                if (item == null)
                    throw new InvalidOperationException($"Item #{line.ItemId} is no longer available.");

                // Business rule validation is unit-tested. The conditional UPDATE is
                // a second guard against another agent consuming stock concurrently.
                _ = InventoryRules.DeductStock(item.StockQuantity, line.Quantity);
                int changed = await _db.Items
                    .Where(x => x.ItemID == line.ItemId && x.StockQuantity >= line.Quantity)
                    .ExecuteUpdateAsync(update => update.SetProperty(
                        x => x.StockQuantity, x => x.StockQuantity - line.Quantity), cancellationToken);
                if (changed != 1)
                    throw new InvalidOperationException($"Insufficient stock for {item.ItemName}. No order was saved.");

                order.OrderDetails.Add(new OrderDetail
                {
                    ItemID = item.ItemID,
                    Quantity = line.Quantity,
                    UnitAmount = item.Price
                });
            }

            // The master and all detail rows are saved while the same DB transaction
            // that deducted every stock quantity is still open.
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return order.OrderID;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
