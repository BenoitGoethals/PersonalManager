namespace PersonnelManager.Application.Abstractions;

public enum LogLevel { Info, Warning, Error }

/// <summary>
/// A logging seam owned by the Application layer, so nothing above Infrastructure knows
/// (or cares) that logs end up in a file. Swap the implementation for a console/database/
/// cloud logger and no caller changes — Dependency Inversion again.
/// </summary>
public interface IAppLogger
{
    void Log(LogLevel level, string message);

    // Default interface methods (C# 8): shorthands every implementer gets for free,
    // so a FileLogger only has to implement the single Log(...) method above.
    void Info(string message) => Log(LogLevel.Info, message);
    void Warning(string message) => Log(LogLevel.Warning, message);
    void Error(string message) => Log(LogLevel.Error, message);
}
