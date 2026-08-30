using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages.Personnel;

[Authorize(Roles = "Admin")]
public sealed class DeleteModel(IPersonnelApiClient api) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public PersonView? Person { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Person = await api.GetByIdAsync(Id, cancellationToken);
        return Person is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await api.DeleteAsync(Id, cancellationToken);
            TempData["Message"] = "Person deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Message"] = $"Delete failed: {ex.Message}";
        }

        return RedirectToPage("/Personnel/Index");
    }
}
