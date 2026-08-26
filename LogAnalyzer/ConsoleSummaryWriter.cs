using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>Writes a LogSummary to the console. Behind ISummaryWriter so it can be swapped or faked.</summary>
public sealed class ConsoleSummaryWriter : ISummaryWriter
{
    public void Write(LogSummary summary)
    {
        var span = summary is { First: { } f, Last: { } l } ? $"{f:HH:mm:ss} → {l:HH:mm:ss}" : "—";
        Console.WriteLine($"» {summary.FileName}  ({summary.ParsedLines} parsed / {summary.UnparsedLines} skipped, {span})");

        // One aligned line of level counts, highest severity first.
        var levels = Enum.GetValues<LogLevel>()
            .Reverse()
            .Where(level => summary.CountsByLevel.GetValueOrDefault(level) > 0)
            .Select(level => $"{level}={summary.CountsByLevel[level]}");
        Console.WriteLine($"    {string.Join("  ", levels)}");

        if (summary.ErrorCount > 0)
        {
            Console.WriteLine($"    ⚠ {summary.ErrorCount} error(s); recent:");
            foreach (var message in summary.RecentErrors)
                Console.WriteLine($"      · {message}");
        }
    }
}
