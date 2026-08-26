using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Application.Personnel;

/// <summary>
/// A rules engine assembled from delegates. Each rule is a FUNCTION — a value you can store in a
/// collection, pass around, and invoke later. That's the whole idea of delegates: behaviour as data.
/// </summary>
public sealed class PersonalValidator : IPersonalValidator
{
    // A rule: given a Personal, hand back an error string, or null if the rule is satisfied.
    // Func<in T, out TResult> is the built-in "function that takes a T and returns a TResult" delegate.
    private static readonly Func<Personal, string?>[] Rules =
    [
        HasAName,                                                  // a METHOD GROUP — a named method used as a delegate
        Rule(p => p.Phone is { Length: > 30 }, "Phone number is too long."),
        Rule(p => p is { Status: EmploymentStatus.Terminated, Name: null },
             "A terminated person must still have a name on record."),
    ];

    public IReadOnlyList<string> Validate(Personal personal) =>
        // Run every rule delegate over the person; OfType<string> drops the nulls (the passes).
        [.. Rules.Select(rule => rule(personal)).OfType<string>()];

    // A named method used above by method-group syntax — no lambda needed to reference it.
    private static string? HasAName(Personal personal) =>
        personal is { Name: null, Surname: null }
            ? "A person needs at least a name or a surname."
            : null;

    // A FACTORY that BUILDS a rule delegate from a Predicate<Personal> (a Func<Personal,bool>) plus
    // a message. The returned lambda CAPTURES `isInvalid` and `message` — that capture is a closure.
    private static Func<Personal, string?> Rule(Predicate<Personal> isInvalid, string message) =>
        personal => isInvalid(personal) ? message : null;
}
