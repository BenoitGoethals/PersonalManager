using PersonnelManager.Domain;
using PersonnelManager.Presentation;

namespace PersonnelManager.Tests;

public class PersonnelDisplayExtensionsTests
{
    [Theory]
    [InlineData(EmploymentStatus.Active, "active")]
    [InlineData(EmploymentStatus.OnLeave, "on leave")]
    [InlineData(EmploymentStatus.Terminated, "terminated")]
    public void Label_InstanceExtensionProperty_ReturnsFriendlyText(EmploymentStatus status, string expected)
    {
        // Called as if EmploymentStatus declared the property itself.
        Assert.Equal(expected, status.Label);
    }

    [Fact]
    public void All_StaticExtensionProperty_ListsEveryStatus()
    {
        // Accessed on the TYPE — a static member added from outside the enum.
        var all = EmploymentStatus.All;

        Assert.Equal(3, all.Count);
        Assert.Contains(EmploymentStatus.OnLeave, all);
    }

    [Fact]
    public void ToDisplayLine_ExtensionMethodOnDto_IncludesFieldsAndLabel()
    {
        var dto = new PersonalDto(Guid.NewGuid(), "Ada", "Lovelace", "London", "+44 100", EmploymentStatus.OnLeave);

        var line = dto.ToDisplayLine();

        Assert.Contains("Ada Lovelace", line);
        Assert.Contains("on leave", line); // uses status.Label internally
    }
}
