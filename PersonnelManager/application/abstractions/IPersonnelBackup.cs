namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// Persists the current personnel to durable storage and restores it again.
/// Both operations do real I/O, so both are asynchronous — they hand back a Task the caller
/// awaits. The CancellationToken lets a caller abort a slow save/load.
/// </summary>
public interface IPersonnelBackup
{
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>Restores saved records into the repository; returns how many were loaded.</summary>
    Task<int> RestoreAsync(CancellationToken cancellationToken = default);
}
