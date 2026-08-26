using LogAnalyzer.Domain;

namespace LogAnalyzer;

/// <summary>Reads a log file and summarizes it.</summary>
public interface ILogFileAnalyzer
{
    Task<LogSummary> AnalyzeAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>Writes synthetic .log files into a folder (for testing).</summary>
public interface ISampleLogGenerator
{
    Task<IReadOnlyList<string>> GenerateAsync(string folder, int fileCount = 10, CancellationToken cancellationToken = default);
}

/// <summary>Renders a summary somewhere (console, in tests: a buffer).</summary>
public interface ISummaryWriter
{
    void Write(LogSummary summary);
}
