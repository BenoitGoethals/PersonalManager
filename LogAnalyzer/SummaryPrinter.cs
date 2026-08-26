using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>Renders a LogSummary to the console.</summary>
public static class SummaryPrinter
{
    public static void Print(LogSummary s)
    {
        var span = s is { First: { } f, Last: { } l } ? $"{f:HH:mm:ss} → {l:HH:mm:ss}" : "—";
        Console.WriteLine($"» {s.FileName}  ({s.ParsedLines} parsed / {s.UnparsedLines} skipped, {span})");

        // One aligned line of level counts, highest severity first.
        var levels = Enum.GetValues<LogLevel>()
            .Reverse()
            .Where(level => s.CountsByLevel.GetValueOrDefault(level) > 0)
            .Select(level => $"{level}={s.CountsByLevel[level]}");
        Console.WriteLine($"    {string.Join("  ", levels)}");

        if (s.ErrorCount > 0)
        {
            Console.WriteLine($"    ⚠ {s.ErrorCount} error(s); recent:");
            foreach (var message in s.RecentErrors)
                Console.WriteLine($"      · {message}");
        }
    }
}
