using PersonnelManager.Application.Abstractions;

namespace PersonnelManager.Infrastructure;

/// <summary>
/// Appends timestamped entries to a log file. Writes are serialized with a lock so concurrent
/// callers can't interleave half-written lines.
/// </summary>
public sealed class FileLogger(string filePath) : IAppLogger
{
    // System.Threading.Lock (C# 13) — a dedicated lock type, clearer than locking a plain object.
    private readonly Lock _gate = new();

    public void Log(LogLevel level, string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

        lock (_gate)
        {
            File.AppendAllText(filePath, line);
        }
    }
}
