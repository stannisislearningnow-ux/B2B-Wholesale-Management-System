using System.Security.Claims;
using B2BWholesale.AgentPortal.Data;
using B2BWholesale.AgentPortal.ViewModels;
using B2BWholesale.Core;
using B2BWholesale.Core.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.AgentPortal.Controllers;

public sealed class AccountController : Controller
{
    private readonly B2BWholesaleDbContext _db;

    public AccountController(B2BWholesaleDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Catalog");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.SingleOrDefaultAsync(x => x.UserName == model.UserName.Trim());
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        if (user.Lock)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Contact the administrator.");
            return View(model);
        }

        if (!PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            if (String.Equals(user.Role, "Agent", StringComparison.OrdinalIgnoreCase))
            {
                user.FailedLoginAttempts = LoginLockoutPolicy.IncrementFailedAttempts(user.FailedLoginAttempts);
                if (LoginLockoutPolicy.ShouldLock(user.FailedLoginAttempts)) user.Lock = true;
                await _db.SaveChangesAsync();
                if (user.Lock)
                    ModelState.AddModelError(string.Empty, "Account locked after five consecutive failed attempts. Contact the administrator.");
                else
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            return View(model);
        }

        if (!String.Equals(user.Role, "Agent", StringComparison.OrdinalIgnoreCase) || !user.AgentID.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Access denied. Only users with the Agent role can use the ordering portal.");
            return View(model);
        }

        user.FailedLoginAttempts = LoginLockoutPolicy.Reset();
        await _db.SaveChangesAsync();

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, "Agent"),
            new Claim("AgentId", user.AgentID.Value.ToString())
        };
        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!String.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);
        return RedirectToAction("Index", "Catalog");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
