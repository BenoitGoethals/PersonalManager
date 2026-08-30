using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages.Personnel;

public sealed class IndexModel(IPersonnelApiClient api) : PageModel
{
    public IReadOnlyList<PersonView> People { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public EmploymentStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Name { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            People = await api.GetAllAsync(Status, Name, cancellationToken);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The API is unreachable. Is PersonnelManager.Api running?";
        }
    }

    public async Task<IActionResult> OnPostBackupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await api.BackupAsync(cancellationToken);
            TempData["Message"] = "Backup saved.";
        }
        catch (ApiException ex)
        {
            TempData["Message"] = $"Backup failed: {ex.Message}";
        }

        return RedirectToPage();
    }
}
