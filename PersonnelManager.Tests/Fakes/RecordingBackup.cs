using PersonnelManager.Application.Abstractions;

namespace PersonnelManager.Tests.Fakes;

/// <summary>
/// A fake IPersonnelBackup that records how often it was asked to save/restore, so a console
/// test can assert the menu option actually triggered the operation — without touching disk.
/// Its async methods complete synchronously via Task.CompletedTask / Task.FromResult.
/// </summary>
public sealed class RecordingBackup : IPersonnelBackup
{
    public int SaveCalls { get; private set; }
    public int RestoreCalls { get; private set; }
    public int RestoreReturns { get; init; }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }

    public Task<int> RestoreAsync(CancellationToken cancellationToken = default)
    {
        RestoreCalls++;
        return Task.FromResult(RestoreReturns);
    }
}
