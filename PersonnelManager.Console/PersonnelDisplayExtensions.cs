using PersonnelManager.Domain;

namespace PersonnelManager.Presentation;

/// <summary>
/// Display-oriented extension members (C# 14). Extension members let you ADD methods, properties,
/// and even STATIC members to a type you don't own — here EmploymentStatus and PersonalDto —
/// without inheritance and without editing the original type.
/// </summary>
public static class PersonnelDisplayExtensions
{
    // INSTANCE extension members: the receiver has a NAME (`status`), so these hang off an instance.
    extension(EmploymentStatus status)
    {
        // Extension PROPERTY — used as `status.Label`, exactly as if EmploymentStatus declared it.
        public string Label => status switch
        {
            EmploymentStatus.Active => "active",
            EmploymentStatus.OnLeave => "on leave",
            EmploymentStatus.Terminated => "terminated",
            _ => status.ToString(),
        };
    }

    // STATIC extension members: the receiver is just the TYPE (no name), so these hang off the type.
    // Called as `EmploymentStatus.All` — a static member bolted onto the enum from outside.
    extension(EmploymentStatus)
    {
        public static IReadOnlyList<EmploymentStatus> All =>
            [EmploymentStatus.Active, EmploymentStatus.OnLeave, EmploymentStatus.Terminated];
    }

    extension(PersonalDto dto)
    {
        // Extension METHOD on the DTO — the single home for the console's one-line rendering.
        public string ToDisplayLine() =>
            $"{dto.Id} | {dto.Name} {dto.Surname} | {dto.Address} | {dto.Phone} | {dto.Status.Label}";
    }
}
