using System.Collections.Concurrent;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure;

/// <summary>
/// A thread-safe in-memory store that works for ANY entity type, written once.
/// This is the payoff of generics: the dictionary, the key logic, the CRUD — none of it
/// is Personal-specific, so it lives here and gets reused for free by every entity.
/// The constraint `where TEntity : IEntity` is what makes `entity.Id` legal below.
/// </summary>
public class InMemoryRepository<TEntity> : IRepository<TEntity>
    where TEntity : IEntity
{
    private readonly ConcurrentDictionary<Guid, TEntity> _store = new();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(id, out var entity) ? entity : default);

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>([.. _store.Values]);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;          // entity.Id — available thanks to the constraint
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_store.ContainsKey(entity.Id))
            return Task.FromResult(false);

        _store[entity.Id] = entity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryRemove(id, out _));
}
