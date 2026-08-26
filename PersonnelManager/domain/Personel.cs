namespace PersonnelManager.Domain;

/// <summary>
/// The Personnel aggregate — the single source of truth for a person in the domain layer.
/// This layer has NO dependencies on any other layer (Dependency Inversion / Clean Architecture).
/// </summary>
public sealed class Personal : IEntity
{
    // Identity is assigned once, at creation, and never changes afterwards.
    // 'init' (C# 9) means it can be set in an object initializer but is immutable thereafter.
    // A get+init property satisfies IEntity's `Guid Id { get; }` requirement.
    public Guid Id { get; init; } = Guid.NewGuid();

    // C# 14 feature: the `field` keyword.
    // It gives you access to the compiler-generated backing field from inside an accessor,
    // so you can add validation/normalization WITHOUT declaring a private backing field yourself.
    // Here every name is trimmed on the way in — invalid state can never be stored.
    public string? Name
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? Surname
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? Address
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? Phone
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    // A new person is Active until told otherwise. Enums make a sensible default trivial.
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
}
