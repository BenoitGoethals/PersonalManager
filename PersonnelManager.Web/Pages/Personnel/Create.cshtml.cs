using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages.Personnel;

public sealed class CreateModel(IPersonnelApiClient api) : PageModel
{
    [BindProperty]
    public PersonInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var created = await api.CreateAsync(Input, cancellationToken);
            TempData["Message"] = $"Created {created.Name} {created.Surname}.";
            return RedirectToPage("/Personnel/Index");
        }
        catch (ApiException ex)
        {
            this.ApplyApiErrors(ex);
            return Page();
        }
    }
}
