using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PersonnelManager.Web.Pages;

[AllowAnonymous]
public sealed class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Personnel/Index");
}
