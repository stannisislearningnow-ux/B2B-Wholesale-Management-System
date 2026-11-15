using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.Models;

public sealed class Item
{
    public int ItemID { get; set; }

    [Required, StringLength(120)]
    public string ItemName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Size { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
