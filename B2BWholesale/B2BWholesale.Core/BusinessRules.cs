using System;
using System.Collections.Generic;
using System.Linq;

namespace B2BWholesale.Core
{
    /// <summary>A requested order line. UnitAmount is the price snapshot used by the order.</summary>
    public sealed class OrderLine
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitAmount { get; set; }
    }

    public static class OrderMath
    {
        public static decimal CalculateTotal(IEnumerable<OrderLine> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            decimal total = 0m;
            foreach (OrderLine line in lines)
            {
                ValidateLine(line);
                total = checked(total + (line.Quantity * line.UnitAmount));
            }
            return total;
        }

        /// <summary>Combines duplicate item IDs so stock is checked against the total quantity.</summary>
        public static IReadOnlyList<OrderLine> MergeDuplicateItems(IEnumerable<OrderLine> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            Dictionary<int, OrderLine> merged = new Dictionary<int, OrderLine>();
            foreach (OrderLine line in lines)
            {
                ValidateLine(line);
                OrderLine existing;
                if (merged.TryGetValue(line.ItemId, out existing))
                {
                    if (existing.UnitAmount != line.UnitAmount)
                        throw new ArgumentException("Duplicate item lines must use the same unit amount.");
                    existing.Quantity = checked(existing.Quantity + line.Quantity);
                }
                else
                {
                    merged.Add(line.ItemId, new OrderLine
                    {
                        ItemId = line.ItemId,
                        Quantity = line.Quantity,
                        UnitAmount = line.UnitAmount
                    });
                }
            }

            return merged.Values.OrderBy(x => x.ItemId).ToList();
        }

        private static void ValidateLine(OrderLine line)
        {
            if (line == null) throw new ArgumentException("Order contains an empty line.");
            if (line.ItemId <= 0) throw new ArgumentOutOfRangeException(nameof(line.ItemId));
            if (line.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(line.Quantity), "Quantity must be greater than zero.");
            if (line.UnitAmount < 0m) throw new ArgumentOutOfRangeException(nameof(line.UnitAmount), "Unit amount cannot be negative.");
        }
    }

    public static class InventoryRules
    {
        /// <summary>Returns the stock remaining after a valid deduction, without changing the input.</summary>
        public static int DeductStock(int currentStock, int requestedQuantity)
        {
            if (currentStock < 0) throw new ArgumentOutOfRangeException(nameof(currentStock));
            if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity), "Quantity must be greater than zero.");
            if (requestedQuantity > currentStock)
                throw new InvalidOperationException("Insufficient stock. The order cannot be placed.");
            return currentStock - requestedQuantity;
        }
    }

    public static class LoginLockoutPolicy
    {
        public const int DefaultMaximumFailedAttempts = 5;

        public static int IncrementFailedAttempts(int currentAttempts, int maximumAttempts = DefaultMaximumFailedAttempts)
        {
            if (currentAttempts < 0) throw new ArgumentOutOfRangeException(nameof(currentAttempts));
            if (maximumAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
            return Math.Min(currentAttempts + 1, maximumAttempts);
        }

        public static bool ShouldLock(int failedAttempts, int maximumAttempts = DefaultMaximumFailedAttempts)
        {
            if (failedAttempts < 0) throw new ArgumentOutOfRangeException(nameof(failedAttempts));
            if (maximumAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
            return failedAttempts >= maximumAttempts;
        }

        public static int Reset() => 0;
    }
}
