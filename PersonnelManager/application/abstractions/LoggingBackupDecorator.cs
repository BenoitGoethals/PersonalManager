namespace PersonnelManager.Application.Abstractions;

/// <summary>
/// Logs around save/restore. Same Decorator pattern as the handler decorators, but IPersonnelBackup
/// isn't generic, so this is a plain (non-generic) wrapper. Callers still see only IPersonnelBackup.
/// </summary>
public sealed class LoggingBackupDecorator(IPersonnelBackup inner, IAppLogger logger)
    : IPersonnelBackup
{
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        logger.Info("Saving personnel to file.");
        try
        {
            await inner.SaveAsync(cancellationToken);
            logger.Info("Saved personnel to file.");
        }
        catch (Exception ex)
        {
            logger.Error($"Save failed: {ex.Message}");
            throw;
        }
    }

    public async Task<int> RestoreAsync(CancellationToken cancellationToken = default)
    {
        logger.Info("Restoring personnel from file.");
        try
        {
            var count = await inner.RestoreAsync(cancellationToken);
            logger.Info($"Restored {count} record(s) from file.");
            return count;
        }
        catch (Exception ex)
        {
            logger.Error($"Restore failed: {ex.Message}");
            throw;
        }
    }
}
