namespace B2BWholesale.AgentPortal.ViewModels;

public sealed class InvoiceViewModel
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentAddress { get; set; } = string.Empty;
    public string AgentPhone { get; set; } = string.Empty;
    public List<InvoiceLineViewModel> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(x => x.LineTotal);
}

public sealed class InvoiceLineViewModel
{
    public string ItemName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitAmount { get; set; }
    public decimal LineTotal => Quantity * UnitAmount;
}
