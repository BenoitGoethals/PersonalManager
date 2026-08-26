using PersonnelManager.Domain;

namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// Validates a Personal and returns one message per broken rule (empty list = valid).
/// </summary>
public interface IPersonalValidator
{
    IReadOnlyList<string> Validate(Personal personal);
}
