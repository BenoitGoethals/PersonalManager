using System.ComponentModel.DataAnnotations;

namespace PersonnelManager.Web.ApiClient;

/// <summary>Employment lifecycle state — mirrors the API's enum (serialized by name).</summary>
public enum EmploymentStatus
{
    Active,
    OnLeave,
    Terminated,
}

/// <summary>A person as returned by the API.</summary>
public record PersonView(
    Guid Id, string? Name, string? Surname, string? Address, string? Phone, EmploymentStatus Status);

/// <summary>The token payload returned by <c>POST /api/auth/login</c>.</summary>
public record TokenResponse(string AccessToken, DateTime ExpiresAtUtc, string TokenType);

/// <summary>Form model for creating/editing a person (with client-side data annotations).</summary>
public sealed class PersonInput
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(200)]
    public string? Surname { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
}
