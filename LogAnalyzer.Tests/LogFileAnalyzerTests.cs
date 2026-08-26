using LogAnalyzer;
using LogAnalyzer.Domain;

namespace LogAnalyzer.Tests;

public class LogFileAnalyzerTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"la-{Guid.NewGuid():N}.log");

    [Fact]
    public async Task Counts_Levels_ParsedAndUnparsed()
    {
        await File.WriteAllTextAsync(_path,
            """
            2026-08-26 09:00:00.000 [Info] a
            2026-08-26 09:00:01.000 [Warning] b
            2026-08-26 09:00:02.000 [Error] boom
            2026-08-26 09:00:03.000 [Fatal] worse
            not a valid line
            """);

        var summary = await LogFileAnalyzer.AnalyzeAsync(_path);

        Assert.Equal(5, summary.TotalLines);
        Assert.Equal(4, summary.ParsedLines);
        Assert.Equal(1, summary.UnparsedLines);
        Assert.Equal(1, summary.CountsByLevel[LogLevel.Warning]);
        Assert.Equal(2, summary.ErrorCount); // Error + Fatal
    }

    [Fact]
    public async Task Captures_TimeSpan_AndRecentErrors()
    {
        await File.WriteAllTextAsync(_path,
            """
            2026-08-26 09:00:00.000 [Info] start
            2026-08-26 09:05:00.000 [Error] disk full
            2026-08-26 09:10:00.000 [Info] end
            """);

        var summary = await LogFileAnalyzer.AnalyzeAsync(_path);

        Assert.Equal(new DateTime(2026, 8, 26, 9, 0, 0), summary.First);
        Assert.Equal(new DateTime(2026, 8, 26, 9, 10, 0), summary.Last);
        Assert.Contains("disk full", summary.RecentErrors);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
