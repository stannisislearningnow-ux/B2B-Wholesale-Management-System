using System.ComponentModel.DataAnnotations;

namespace B2BWholesale.AgentPortal.ViewModels;

public sealed class PlaceOrderViewModel
{
    public List<OrderLineInputViewModel> Lines { get; set; } = new();
}

public sealed class OrderLineInputViewModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    [Range(0, 1000000, ErrorMessage = "Quantity must be zero or a positive number.")]
    public int Quantity { get; set; }
}
