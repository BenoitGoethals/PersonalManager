using FluentValidation;
using PersonnelManager.Api.Contracts;

namespace PersonnelManager.Api.Validation;

/// <summary>
/// Validators for the API request DTOs. These guard the HTTP boundary (shape, lengths,
/// required fields) and run before the request reaches the application service, which then
/// enforces the deeper domain invariants (see PersonalValidator in the core library).
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty();
        RuleFor(request => request.Password).NotEmpty();
    }
}

public sealed class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
{
    public CreatePersonRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.Name) || !string.IsNullOrWhiteSpace(request.Surname))
            .WithMessage("A person needs at least a name or a surname.");

        RuleFor(request => request.Name).MaximumLength(200);
        RuleFor(request => request.Surname).MaximumLength(200);
        RuleFor(request => request.Address).MaximumLength(500);
        RuleFor(request => request.Phone).MaximumLength(30);
        RuleFor(request => request.Status).IsInEnum();
    }
}

public sealed class UpdatePersonRequestValidator : AbstractValidator<UpdatePersonRequest>
{
    public UpdatePersonRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => !string.IsNullOrWhiteSpace(request.Name) || !string.IsNullOrWhiteSpace(request.Surname))
            .WithMessage("A person needs at least a name or a surname.");

        RuleFor(request => request.Name).MaximumLength(200);
        RuleFor(request => request.Surname).MaximumLength(200);
        RuleFor(request => request.Address).MaximumLength(500);
        RuleFor(request => request.Phone).MaximumLength(30);
        RuleFor(request => request.Status).IsInEnum();
    }
}

public sealed class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    public ChangeStatusRequestValidator() => RuleFor(request => request.Status).IsInEnum();
}
