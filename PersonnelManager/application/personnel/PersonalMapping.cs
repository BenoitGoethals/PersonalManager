using PersonnelManager.Domain;

namespace PersonnelManager.Application.Personnel;

/// <summary>
/// Maps between the domain entity and the DTO.
/// Showcases the C# 14 "extension members" feature: the new `extension(...)` block lets you
/// declare extension methods AND extension properties in one place, with the receiver named
/// once at the top instead of repeating `this Personal person` on every method.
/// </summary>
public static class PersonalMapping
{
    extension(Personal person)
    {
        // Extension method (C# 14 block form).
        public PersonalDto ToDto() =>
            new(person.Id, person.Name, person.Surname, person.Address, person.Phone, person.Status);

        // Extension PROPERTY — not possible before C# 14. Computed over the receiver.
        public string DisplayName =>
            string.Join(' ', new[] { person.Name, person.Surname }
                .Where(part => !string.IsNullOrWhiteSpace(part)));

        public bool IsComplete =>
            person is { Name: not null, Surname: not null };
    }
}
