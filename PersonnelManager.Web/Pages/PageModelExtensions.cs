using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Pages;

public static class PageModelExtensions
{
    /// <summary>Surface an API error on the page: validation errors become model errors, others a summary error.</summary>
    public static void ApplyApiErrors(this PageModel page, ApiException exception)
    {
        if (exception.IsValidationError)
            foreach (var (_, messages) in exception.Errors)
                foreach (var message in messages)
                    page.ModelState.AddModelError(string.Empty, message);
        else
            page.ModelState.AddModelError(string.Empty, exception.Message);
    }
}
