namespace LogAnalyzer.Domain;

/// <summary>Severity levels a log line can carry, low to high.</summary>
public enum LogLevel { Trace, Debug, Info, Warning, Error, Fatal }

/// <summary>One parsed log line.</summary>
public record LogEntry(DateTime Timestamp, LogLevel Level, string Message);

/// <summary>The result of analysing a single log file.</summary>
public record LogSummary(
    string FileName,
    int TotalLines,
    int ParsedLines,
    int UnparsedLines,
    IReadOnlyDictionary<LogLevel, int> CountsByLevel,
    DateTime? First,
    DateTime? Last,
    IReadOnlyList<string> RecentErrors)
{
    public int ErrorCount => CountsByLevel.GetValueOrDefault(LogLevel.Error)
                           + CountsByLevel.GetValueOrDefault(LogLevel.Fatal);
}
