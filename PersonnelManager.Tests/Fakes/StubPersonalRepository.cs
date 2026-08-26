using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Tests.Fakes;

/// <summary>
/// A hand-rolled test double (no mocking framework needed). It lets a test force the
/// otherwise-unreachable branch in UpdatePersonalHandler where the entity exists but the
/// persistence call reports failure. Behaviour is controlled by the constructor flags.
/// </summary>
public sealed class StubPersonalRepository(Personal? existing = null, bool updateSucceeds = true)
    : IPersonalRepository
{
    public Task<Personal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existing);

    public Task<IReadOnlyList<Personal>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Personal>>(existing is null ? [] : [existing]);

    public Task AddAsync(Personal personal, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<bool> UpdateAsync(Personal personal, CancellationToken cancellationToken = default) =>
        Task.FromResult(updateSucceeds);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(existing is not null);
}
