using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;

namespace PersonnelManager.Tests;

public class PersonalValidatorTests
{
    private readonly PersonalValidator _validator = new();

    [Fact]
    public void ValidPerson_HasNoErrors()
    {
        var errors = _validator.Validate(new Personal { Name = "Ada", Surname = "Lovelace" });

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingNameAndSurname_FailsTheNameRule()
    {
        var errors = _validator.Validate(new Personal()); // no name, no surname

        Assert.Contains(errors, e => e.Contains("name or a surname"));
    }

    [Fact]
    public void OverlongPhone_FailsThePhoneRule()
    {
        var errors = _validator.Validate(new Personal
        {
            Name = "Ada", Phone = new string('9', 40),
        });

        Assert.Contains(errors, e => e.Contains("Phone number is too long"));
    }

    [Fact]
    public void TerminatedWithNoName_FailsThatRule()
    {
        var errors = _validator.Validate(new Personal
        {
            Surname = "Turing", Status = EmploymentStatus.Terminated,
        });

        Assert.Contains(errors, e => e.Contains("terminated person"));
    }

    [Fact]
    public void MultipleBrokenRules_ReturnAllMessages()
    {
        // No name/surname AND an over-long phone → two independent rule delegates fire.
        var errors = _validator.Validate(new Personal { Phone = new string('9', 40) });

        Assert.Equal(2, errors.Count);
    }
}
