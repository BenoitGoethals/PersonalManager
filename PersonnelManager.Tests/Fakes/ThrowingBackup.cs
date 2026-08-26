using PersonnelManager.Application.Abstractions;

namespace PersonnelManager.Tests.Fakes;

/// <summary>An IPersonnelBackup that always fails — used to test the decorator's error path.</summary>
public sealed class ThrowingBackup : IPersonnelBackup
{
    public Task SaveAsync(CancellationToken cancellationToken = default) =>
        throw new IOException("disk full");

    public Task<int> RestoreAsync(CancellationToken cancellationToken = default) =>
        throw new IOException("disk full");
}
