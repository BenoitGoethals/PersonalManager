using PersonnelManager.Application.Abstractions;

namespace PersonnelManager.Tests.Fakes;

/// <summary>An in-memory IAppLogger that keeps every entry so tests can assert on them.</summary>
public sealed class RecordingLogger : IAppLogger
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public void Log(LogLevel level, string message) => Entries.Add((level, message));
}
