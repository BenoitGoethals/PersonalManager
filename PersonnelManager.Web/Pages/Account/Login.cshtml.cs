using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(IPersonnelApiClient api) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        TokenResponse token;
        try
        {
            token = await api.LoginAsync(Input.Username, Input.Password, cancellationToken);
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API is unreachable. Is PersonnelManager.Api running?");
            return Page();
        }

        // Build the local auth cookie from the JWT's claims, stashing the raw token so the
        // BearerTokenHandler can attach it to subsequent API calls.
        var claims = new List<Claim> { new(BearerTokenHandler.AccessTokenClaim, token.AccessToken) };
        claims.AddRange(JwtReader.ReadClaims(token.AccessToken));
        if (!claims.Any(c => c.Type == ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, Input.Username));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { ExpiresUtc = token.ExpiresAtUtc, IsPersistent = true });

        return LocalRedirect(returnUrl ?? "/Personnel/Index");
    }
}
