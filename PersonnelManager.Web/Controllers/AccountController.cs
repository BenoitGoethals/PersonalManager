using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Web.ApiClient;
using PersonnelManager.Web.Models;

namespace PersonnelManager.Web.Controllers;

[AllowAnonymous]
public sealed class AccountController(IPersonnelApiClient api) : Controller
{
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel input, string? returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(input);

        TokenResponse token;
        try
        {
            token = await api.LoginAsync(input.Username, input.Password, cancellationToken);
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(input);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API is unreachable. Is PersonnelManager.Api running?");
            return View(input);
        }

        // Build the local auth cookie from the JWT's claims, stashing the raw token so the
        // BearerTokenHandler can attach it to subsequent API calls.
        var claims = new List<Claim> { new(BearerTokenHandler.AccessTokenClaim, token.AccessToken) };
        claims.AddRange(JwtReader.ReadClaims(token.AccessToken));
        if (!claims.Any(c => c.Type == ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, input.Username));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { ExpiresUtc = token.ExpiresAtUtc, IsPersistent = true });

        return LocalRedirect(returnUrl ?? "/Personnel/Index");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // A GET to /Account/Logout just sends the user home rather than erroring.
    [HttpGet]
    [ActionName("Logout")]
    public IActionResult LogoutGet() => RedirectToAction("Index", "Personnel");
}
