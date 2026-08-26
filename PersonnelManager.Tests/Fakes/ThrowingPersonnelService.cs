using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Tests.Fakes;

/// <summary>An IPersonnelService that always throws — used to test the decorator's error path.</summary>
public sealed class ThrowingPersonnelService : IPersonnelService
{
    public Task<Result<PersonalDto>> CreateAsync(CreatePersonalRequest request, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("boom");

    public Task<Result<PersonalDto>> UpdateAsync(UpdatePersonalRequest request, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("boom");

    public Task<Result<Guid>> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("boom");

    public Task<Result<PersonalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("boom");

    public Task<IReadOnlyList<PersonalDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("boom");
}
