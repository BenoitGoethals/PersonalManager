using FluentValidation;
using PersonnelManager.Domain;

namespace PersonnelManager.Application.Personnel;

/// <summary>
/// The personnel rule set, expressed with FluentValidation. Each <c>RuleFor</c> is a small,
/// declarative rule; FluentValidation runs them all and collects one failure per broken rule
/// (its default cascade mode is Continue, so every rule is evaluated independently).
/// The service depends on the framework's <see cref="IValidator{T}"/>, so this class is the
/// single place the personnel invariants live.
/// </summary>
public sealed class PersonalValidator : AbstractValidator<Personal>
{
    public PersonalValidator()
    {
        // A person needs at least one of name/surname. The rule is on the whole entity because it
        // spans two properties. (Name/Surname are normalised to null when blank by the domain.)
        RuleFor(personal => personal)
            .Must(personal => personal.Name is not null || personal.Surname is not null)
            .WithMessage("A person needs at least a name or a surname.");

        // MaximumLength passes on null, so this only bites when a phone is actually present.
        RuleFor(personal => personal.Phone)
            .MaximumLength(30)
            .WithMessage("Phone number is too long.");

        // A terminated person must still carry a name on record.
        RuleFor(personal => personal.Name)
            .NotNull()
            .When(personal => personal.Status == EmploymentStatus.Terminated)
            .WithMessage("A terminated person must still have a name on record.");
    }
}
