using PersonnelManager.Domain;

namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// The persistence contract for the Personal aggregate. It's now just IRepository&lt;Personal&gt;
/// with a name — a "closed" version of the generic interface. Keeping a named interface
/// (rather than using IRepository&lt;Personal&gt; everywhere) gives us a home for Personal-only
/// queries later, and keeps the Dependency Inversion story readable.
/// </summary>
public interface IPersonalRepository : IRepository<Personal>
{
    // e.g. Task<IReadOnlyList<Personal>> FindBySurnameAsync(string surname, ...);
}
