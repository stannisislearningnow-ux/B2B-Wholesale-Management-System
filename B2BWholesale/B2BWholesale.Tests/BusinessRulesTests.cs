using B2BWholesale.Core;
using B2BWholesale.Core.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2BWholesale.Tests;

[TestClass]
public class BusinessRulesTests
{
    [TestMethod]
    public void DeductStock_WhenQuantityIsAvailable_ReturnsRemainingStock()
    {
        int remaining = InventoryRules.DeductStock(20, 7);
        Assert.AreEqual(13, remaining);
    }

    [TestMethod]
    public void DeductStock_WhenQuantityExceedsStock_RejectsOrder()
    {
        Assert.ThrowsException<InvalidOperationException>(() => InventoryRules.DeductStock(4, 5));
    }

    [TestMethod]
    public void DeductStock_WhenQuantityIsZero_RejectsOrder()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => InventoryRules.DeductStock(10, 0));
    }

    [TestMethod]
    public void CalculateTotal_MultipleOrderLines_ReturnsCorrectAmount()
    {
        var lines = new[]
        {
            new OrderLine { ItemId = 1, Quantity = 2, UnitAmount = 125000m },
            new OrderLine { ItemId = 2, Quantity = 3, UnitAmount = 26000m }
        };

        Assert.AreEqual(328000m, OrderMath.CalculateTotal(lines));
    }

    [TestMethod]
    public void MergeDuplicateItems_CombinesQuantitiesForStockValidation()
    {
        var lines = new[]
        {
            new OrderLine { ItemId = 7, Quantity = 2, UnitAmount = 32000m },
            new OrderLine { ItemId = 7, Quantity = 3, UnitAmount = 32000m }
        };

        var merged = OrderMath.MergeDuplicateItems(lines);
        Assert.AreEqual(1, merged.Count);
        Assert.AreEqual(5, merged[0].Quantity);
    }

    [TestMethod]
    public void LoginLockoutPolicy_LocksAccountAtFiveFailedAttempts()
    {
        int attempts = 0;
        for (int i = 0; i < 5; i++)
            attempts = LoginLockoutPolicy.IncrementFailedAttempts(attempts);

        Assert.AreEqual(5, attempts);
        Assert.IsTrue(LoginLockoutPolicy.ShouldLock(attempts));
        Assert.AreEqual(0, LoginLockoutPolicy.Reset());
    }

    [TestMethod]
    public void PasswordHasher_VerifiesCorrectPasswordAndRejectsWrongPassword()
    {
        string hash = PasswordHasher.HashPassword("AgentDemo123!");
        Assert.IsTrue(PasswordHasher.VerifyPassword("AgentDemo123!", hash));
        Assert.IsFalse(PasswordHasher.VerifyPassword("WrongPassword", hash));
    }
}
