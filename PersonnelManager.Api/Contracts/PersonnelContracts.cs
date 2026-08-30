using PersonnelManager.Domain;

namespace PersonnelManager.Api.Contracts;

/// <summary>Body of <c>POST /api/personnel</c> — the fields a client may supply when creating a person.</summary>
public record CreatePersonRequest(
    string? Name,
    string? Surname,
    string? Address,
    string? Phone,
    EmploymentStatus Status = EmploymentStatus.Active);

/// <summary>Body of <c>PUT /api/personnel/{id}</c> — a full replacement of the mutable fields.</summary>
public record UpdatePersonRequest(
    string? Name,
    string? Surname,
    string? Address,
    string? Phone,
    EmploymentStatus Status = EmploymentStatus.Active);

/// <summary>Body of <c>PATCH /api/personnel/{id}/status</c> — change only the employment status.</summary>
public record ChangeStatusRequest(EmploymentStatus Status);
