namespace B2BWholesale.AgentPortal.ViewModels;

public sealed class OrderSummaryViewModel
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
