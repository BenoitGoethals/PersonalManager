namespace PersonnelManager.Domain;

/// <summary>
/// A read/transfer model that crosses layer boundaries (Application -> Presentation).
/// Records (C# 9+) give you value equality and immutability for free — ideal for DTOs.
/// The entity never leaves the domain; callers only ever see this shape.
/// </summary>
public record PersonalDto(
    Guid Id, string? Name, string? Surname, string? Address, string? Phone, EmploymentStatus Status);
