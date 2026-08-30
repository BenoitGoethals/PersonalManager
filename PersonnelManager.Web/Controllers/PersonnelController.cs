using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonnelManager.Web.ApiClient;
using PersonnelManager.Web.Models;

namespace PersonnelManager.Web.Controllers;

[Authorize]
public sealed class PersonnelController(IPersonnelApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(EmploymentStatus? status, string? name, CancellationToken cancellationToken)
    {
        IReadOnlyList<PersonView> people = [];
        string? errorMessage = null;

        try
        {
            people = await api.GetAllAsync(status, name, cancellationToken);
        }
        catch (ApiException ex)
        {
            errorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            errorMessage = "The API is unreachable. Is PersonnelManager.Api running?";
        }

        return View(new PersonnelIndexViewModel
        {
            People = people,
            Status = status,
            Name = name,
            ErrorMessage = errorMessage,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Backup(CancellationToken cancellationToken)
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

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Create() => View(new PersonInput());

    [HttpPost]
    public async Task<IActionResult> Create(PersonInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(input);

        try
        {
            var created = await api.CreateAsync(input, cancellationToken);
            TempData["Message"] = $"Created {created.Name} {created.Surname}.";
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            this.ApplyApiErrors(ex);
            return View(input);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var person = await api.GetByIdAsync(id, cancellationToken);
        if (person is null)
            return NotFound();

        var input = new PersonInput
        {
            Name = person.Name,
            Surname = person.Surname,
            Address = person.Address,
            Phone = person.Phone,
            Status = person.Status,
        };
        ViewData["Id"] = id;
        return View(input);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, PersonInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Id"] = id;
            return View(input);
        }

        try
        {
            await api.UpdateAsync(id, input, cancellationToken);
            TempData["Message"] = "Changes saved.";
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            this.ApplyApiErrors(ex);
            ViewData["Id"] = id;
            return View(input);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var person = await api.GetByIdAsync(id, cancellationToken);
        return person is null ? NotFound() : View(person);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await api.DeleteAsync(id, cancellationToken);
            TempData["Message"] = "Person deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Message"] = $"Delete failed: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
}
