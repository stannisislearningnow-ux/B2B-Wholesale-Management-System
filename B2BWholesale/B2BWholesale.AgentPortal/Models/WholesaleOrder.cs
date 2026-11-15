using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.Models;

public sealed class WholesaleOrder
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public int AgentID { get; set; }

    [Required, StringLength(20)]
    public string OrderStatus { get; set; } = "Pending";

    public Agent? Agent { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
