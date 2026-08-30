using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages.Personnel;

public sealed class EditModel(IPersonnelApiClient api) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public PersonInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var person = await api.GetByIdAsync(Id, cancellationToken);
        if (person is null)
            return NotFound();

        Input = new PersonInput
        {
            Name = person.Name,
            Surname = person.Surname,
            Address = person.Address,
            Phone = person.Phone,
            Status = person.Status,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await api.UpdateAsync(Id, Input, cancellationToken);
            TempData["Message"] = "Changes saved.";
            return RedirectToPage("/Personnel/Index");
        }
        catch (ApiException ex)
        {
            this.ApplyApiErrors(ex);
            return Page();
        }
    }
}
