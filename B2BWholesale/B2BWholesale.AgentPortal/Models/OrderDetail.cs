using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.Models;

public sealed class OrderDetail
{
    public int ID { get; set; }
    public int OrderID { get; set; }
    public int ItemID { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal UnitAmount { get; set; }

    public WholesaleOrder? Order { get; set; }
    public Item? Item { get; set; }
}
