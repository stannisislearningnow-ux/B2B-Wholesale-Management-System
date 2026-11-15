using Microsoft.AspNetCore.Mvc;

namespace B2BWholesale.AgentPortal.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Catalog")
        : RedirectToAction("Login", "Account");

    public IActionResult Error() => View();
}
