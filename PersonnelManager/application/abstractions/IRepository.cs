using PersonnelManager.Domain;

namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// A reusable persistence contract for ANY entity, not just Personal.
/// `TEntity` is a type PARAMETER — a placeholder filled in at each use site
/// (IRepository&lt;Personal&gt;, IRepository&lt;Order&gt;, ...). The `where` clause is a generic
/// CONSTRAINT: it guarantees every TEntity has an Id, so the implementation can key on it.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
