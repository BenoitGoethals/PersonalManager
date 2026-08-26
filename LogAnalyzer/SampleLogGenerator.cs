using System.Text;
using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>Writes synthetic .log files (matching the analyzer's format) into a target folder.</summary>
public static class SampleLogGenerator
{
    private static readonly string[] Services = ["auth", "billing", "orders", "search", "gateway"];
    private static readonly string[] Messages =
    [
        "request received", "cache miss", "user authenticated", "payload validated",
        "db query executed", "retrying upstream call", "timeout waiting for response",
        "connection reset by peer", "record not found", "rate limit exceeded",
    ];

    public static async Task<IReadOnlyList<string>> GenerateAsync(
        string folder, int fileCount = 10, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(folder);
        var written = new List<string>();
        var start = DateTime.Now.AddHours(-fileCount);

        for (var f = 0; f < fileCount; f++)
        {
            var service = Services[f % Services.Length];
            var path = Path.Combine(folder, $"{service}-{f + 1:D2}.log");
            var lines = BuildFile(service, start.AddHours(f), Random.Shared.Next(15, 40));

            await File.WriteAllTextAsync(path, lines, cancellationToken);
            written.Add(path);
        }

        return written;
    }

    private static string BuildFile(string service, DateTime start, int lineCount)
    {
        var sb = new StringBuilder();
        var when = start;

        for (var i = 0; i < lineCount; i++)
        {
            when = when.AddSeconds(Random.Shared.Next(1, 90));
            var level = WeightedLevel();
            var message = $"[{service}] {Messages[Random.Shared.Next(Messages.Length)]}";
            sb.AppendLine($"{when:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}");
        }

        // Sprinkle in one malformed line so "unparsed" counting is exercised.
        sb.AppendLine("### rotated log segment — not a standard line ###");
        return sb.ToString();
    }

    // Skew towards Info; errors are rarer, like real traffic.
    private static LogLevel WeightedLevel() => Random.Shared.Next(100) switch
    {
        < 55 => LogLevel.Info,
        < 70 => LogLevel.Debug,
        < 80 => LogLevel.Trace,
        < 92 => LogLevel.Warning,
        < 99 => LogLevel.Error,
        _ => LogLevel.Fatal,
    };
}
