using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Models;

public sealed class PersonnelIndexViewModel
{
    public IReadOnlyList<PersonView> People { get; init; } = [];

    public EmploymentStatus? Status { get; init; }

    public string? Name { get; init; }

    public string? ErrorMessage { get; init; }
}
