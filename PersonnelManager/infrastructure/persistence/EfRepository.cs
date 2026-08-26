using Microsoft.EntityFrameworkCore;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure.Persistence;

/// <summary>
/// A reusable EF Core repository for any IEntity. It creates a short-lived DbContext per operation
/// via IDbContextFactory — the recommended pattern for apps without a per-request scope. It mirrors
/// the same IRepository&lt;TEntity&gt; contract the in-memory store implements, so callers are unaffected.
/// </summary>
public class EfRepository<TEntity>(IDbContextFactory<PersonnelDbContext> contextFactory)
    : IRepository<TEntity>
    where TEntity : class, IEntity
{
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        // FindAsync looks up by primary key — no expression over the interface member needed.
        return await db.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.Set<TEntity>().Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.Set<TEntity>().FindAsync([entity.Id], cancellationToken);
        if (existing is null)
            return false;

        // Copy the incoming scalar values onto the tracked entity, then save.
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.Set<TEntity>().FindAsync([id], cancellationToken);
        if (existing is null)
            return false;

        db.Set<TEntity>().Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
