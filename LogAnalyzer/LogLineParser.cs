using System.Globalization;
using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>
/// Parses a single log line of the form:  yyyy-MM-dd HH:mm:ss.fff [Level] message
/// (the exact format PersonnelManager's FileLogger writes). Returns false for anything
/// that doesn't match — the analyzer counts those as "unparsed" rather than crashing.
/// </summary>
public static class LogLineParser
{
    public static bool TryParse(string line, out LogEntry entry)
    {
        entry = default!;
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var open = line.IndexOf('[');
        var close = line.IndexOf(']');
        if (open < 0 || close < 0 || close < open)
            return false;

        var timestampText = line[..open].Trim();
        var levelText = line[(open + 1)..close].Trim();
        var message = line[(close + 1)..].Trim();

        if (!DateTime.TryParse(timestampText, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var timestamp))
            return false;

        if (!TryParseLevel(levelText, out var level))
            return false;

        entry = new LogEntry(timestamp, level, message);
        return true;
    }

    private static bool TryParseLevel(string text, out LogLevel level)
    {
        // Accept the enum names plus the common shorthands seen in real logs.
        level = text.ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" or "DBG" => LogLevel.Debug,
            "INFO" or "INFORMATION" => LogLevel.Info,
            "WARN" or "WARNING" => LogLevel.Warning,
            "ERR" or "ERROR" => LogLevel.Error,
            "FATAL" or "CRIT" or "CRITICAL" => LogLevel.Fatal,
            _ => (LogLevel)(-1),
        };
        return Enum.IsDefined(level);
    }
}
