using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Controllers;

public static class ControllerExtensions
{
    /// <summary>Surface an API error on the controller: validation errors become model errors, others a summary error.</summary>
    public static void ApplyApiErrors(this Controller controller, ApiException exception)
    {
        if (exception.IsValidationError)
            foreach (var (_, messages) in exception.Errors)
                foreach (var message in messages)
                    controller.ModelState.AddModelError(string.Empty, message);
        else
            controller.ModelState.AddModelError(string.Empty, exception.Message);
    }
}
