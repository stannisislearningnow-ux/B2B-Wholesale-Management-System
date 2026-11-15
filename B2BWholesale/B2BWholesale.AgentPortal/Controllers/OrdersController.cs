using B2BWholesale.AgentPortal.Data;
using B2BWholesale.AgentPortal.Services;
using B2BWholesale.AgentPortal.ViewModels;
using B2BWholesale.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Controllers;

[Authorize(Roles = "Agent")]
public sealed class OrdersController : Controller
{
    private readonly B2BWholesaleDbContext _db;
    private readonly OrderService _orderService;

    public OrdersController(B2BWholesaleDbContext db, OrderService orderService)
    {
        _db = db;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        int agentId = CurrentAgentId();
        var orders = await _db.Orders.AsNoTracking()
            .Where(x => x.AgentID == agentId)
            .Include(x => x.OrderDetails)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();

        List<OrderSummaryViewModel> model = orders.Select(order => new OrderSummaryViewModel
        {
            OrderID = order.OrderID,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            Total = OrderMath.CalculateTotal(order.OrderDetails.Select(line => new OrderLine
            {
                ItemId = line.ItemID,
                Quantity = line.Quantity,
                UnitAmount = line.UnitAmount
            }))
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Place()
    {
        return View(await BuildPlaceOrderViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(PlaceOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RefreshItemDisplayFieldsAsync(model);
            return View(model);
        }

        List<OrderLine> requested = model.Lines
            .Where(x => x.Quantity > 0)
            .Select(x => new OrderLine { ItemId = x.ItemId, Quantity = x.Quantity, UnitAmount = 0m })
            .ToList();

        try
        {
            int orderId = await _orderService.PlaceOrderAsync(CurrentAgentId(), requested);
            TempData["SuccessMessage"] = $"Order #{orderId} was placed successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await RefreshItemDisplayFieldsAsync(model);
            return View(model);
        }
    }

    public async Task<IActionResult> Invoice(int id)
    {
        int agentId = CurrentAgentId();
        var order = await _db.Orders.AsNoTracking()
            .Include(x => x.Agent)
            .Include(x => x.OrderDetails).ThenInclude(x => x.Item)
            .SingleOrDefaultAsync(x => x.OrderID == id && x.AgentID == agentId);

        // The AgentID condition prevents an agent from opening another agent's invoice.
        if (order == null || order.Agent == null) return NotFound();

        InvoiceViewModel model = new()
        {
            OrderID = order.OrderID,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            AgentName = order.Agent.AgentName,
            AgentAddress = order.Agent.Address,
            AgentPhone = order.Agent.Phone,
            Lines = order.OrderDetails.Select(detail => new InvoiceLineViewModel
            {
                ItemName = detail.Item?.ItemName ?? "Item",
                Size = detail.Item?.Size ?? string.Empty,
                Quantity = detail.Quantity,
                UnitAmount = detail.UnitAmount
            }).ToList()
        };
        return View(model);
    }

    private int CurrentAgentId()
    {
        string? value = User.FindFirst("AgentId")?.Value;
        if (!Int32.TryParse(value, out int agentId) || agentId <= 0)
            throw new InvalidOperationException("The signed-in account is not linked to an Agent.");
        return agentId;
    }

    private async Task<PlaceOrderViewModel> BuildPlaceOrderViewModelAsync()
    {
        var items = await _db.Items.AsNoTracking().OrderBy(x => x.ItemName).ToListAsync();
        return new PlaceOrderViewModel
        {
            Lines = items.Select(item => new OrderLineInputViewModel
            {
                ItemId = item.ItemID,
                ItemName = item.ItemName,
                Size = item.Size,
                Price = item.Price,
                StockQuantity = item.StockQuantity
            }).ToList()
        };
    }

    private async Task RefreshItemDisplayFieldsAsync(PlaceOrderViewModel model)
    {
        var items = await _db.Items.AsNoTracking().ToDictionaryAsync(x => x.ItemID);
        foreach (var line in model.Lines)
        {
            if (items.TryGetValue(line.ItemId, out var item))
            {
                line.ItemName = item.ItemName;
                line.Size = item.Size;
                line.Price = item.Price;
                line.StockQuantity = item.StockQuantity;
            }
        }
    }
}
