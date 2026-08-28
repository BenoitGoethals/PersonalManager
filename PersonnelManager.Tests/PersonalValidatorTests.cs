using FluentValidation.TestHelper;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;

namespace PersonnelManager.Tests;

public class PersonalValidatorTests
{
    private readonly PersonalValidator _validator = new();

    [Fact]
    public void ValidPerson_HasNoErrors()
    {
        var result = _validator.TestValidate(new Personal { Name = "Ada", Surname = "Lovelace" });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MissingNameAndSurname_FailsTheNameRule()
    {
        var result = _validator.TestValidate(new Personal()); // no name, no surname

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("name or a surname"));
    }

    [Fact]
    public void OverlongPhone_FailsThePhoneRule()
    {
        var result = _validator.TestValidate(new Personal
        {
            Name = "Ada", Phone = new string('9', 40),
        });

        result.ShouldHaveValidationErrorFor(p => p.Phone)
            .WithErrorMessage("Phone number is too long.");
    }

    [Fact]
    public void TerminatedWithNoName_FailsThatRule()
    {
        var result = _validator.TestValidate(new Personal
        {
            Surname = "Turing", Status = EmploymentStatus.Terminated,
        });

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("terminated person"));
    }

    [Fact]
    public void MultipleBrokenRules_ReturnAllMessages()
    {
        // No name/surname AND an over-long phone → two independent rules fire.
        var result = _validator.TestValidate(new Personal { Phone = new string('9', 40) });

        Assert.Equal(2, result.Errors.Count);
    }
}
