using LogAnalyzer;
using LogAnalyzer.Domain;

namespace LogAnalyzer.Tests;

public class LogLineParserTests
{
    [Fact]
    public void Parses_AWellFormedLine()
    {
        var ok = LogLineParser.TryParse("2026-08-26 09:17:22.952 [Info] service started", out var entry);

        Assert.True(ok);
        Assert.Equal(LogLevel.Info, entry.Level);
        Assert.Equal("service started", entry.Message);
        Assert.Equal(new DateTime(2026, 8, 26, 9, 17, 22, 952), entry.Timestamp);
    }

    [Theory]
    [InlineData("WARN", LogLevel.Warning)]
    [InlineData("error", LogLevel.Error)]
    [InlineData("CRITICAL", LogLevel.Fatal)]
    public void Accepts_LevelAliases_CaseInsensitively(string level, LogLevel expected)
    {
        var ok = LogLineParser.TryParse($"2026-08-26 09:17:22.952 [{level}] x", out var entry);

        Assert.True(ok);
        Assert.Equal(expected, entry.Level);
    }

    [Theory]
    [InlineData("")]
    [InlineData("### rotated segment — not a log line ###")]
    [InlineData("2026-99-99 xx [Info] bad timestamp")]
    [InlineData("2026-08-26 09:17:22.952 [Nonsense] unknown level")]
    public void Rejects_MalformedLines(string line)
    {
        Assert.False(LogLineParser.TryParse(line, out _));
    }
}
