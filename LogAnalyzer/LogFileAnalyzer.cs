using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>Reads a log file and summarizes it. Pure I/O + counting, no console concerns.</summary>
public sealed class LogFileAnalyzer : ILogFileAnalyzer
{
    public async Task<LogSummary> AnalyzeAsync(string path, CancellationToken cancellationToken = default)
    {
        var counts = new Dictionary<LogLevel, int>();
        var recentErrors = new List<string>();
        int total = 0, parsed = 0, unparsed = 0;
        DateTime? first = null, last = null;

        foreach (var line in await ReadLinesAsync(path, cancellationToken))
        {
            total++;
            if (!LogLineParser.TryParse(line, out var entry))
            {
                unparsed++;
                continue;
            }

            parsed++;
            counts[entry.Level] = counts.GetValueOrDefault(entry.Level) + 1;
            first ??= entry.Timestamp;
            last = entry.Timestamp;

            if (entry.Level is LogLevel.Error or LogLevel.Fatal)
                recentErrors.Add(entry.Message);
        }

        // Keep only the last few error messages for the summary.
        var lastErrors = recentErrors.Count <= 3 ? recentErrors : recentErrors[^3..];

        return new LogSummary(
            FileName: Path.GetFileName(path),
            TotalLines: total,
            ParsedLines: parsed,
            UnparsedLines: unparsed,
            CountsByLevel: counts,
            First: first,
            Last: last,
            RecentErrors: lastErrors);
    }

    // Open with FileShare.ReadWrite so we can read a file that is still being written to
    // (the watcher often fires while a writer still holds the handle).
    private static async Task<string[]> ReadLinesAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        var lines = new List<string>();
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
            lines.Add(line);

        return [.. lines];
    }
}
