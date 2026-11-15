using B2BWholesale.AgentPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Controllers;

[Authorize(Roles = "Agent")]
public sealed class CatalogController : Controller
{
    private readonly B2BWholesaleDbContext _db;
    public CatalogController(B2BWholesaleDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var items = await _db.Items.AsNoTracking().OrderBy(x => x.ItemName).ToListAsync();
        return View(items);
    }
}
