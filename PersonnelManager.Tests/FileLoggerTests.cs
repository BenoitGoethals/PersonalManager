using PersonnelManager.Application.Abstractions;
using PersonnelManager.Infrastructure;

namespace PersonnelManager.Tests;

public class FileLoggerTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid():N}.log");

    [Fact]
    public void Log_WritesLevelAndMessage_ToFile()
    {
        // Typed as IAppLogger: the Info/Error shorthands are DEFAULT INTERFACE METHODS,
        // reachable only through the interface, not through the concrete FileLogger type.
        IAppLogger logger = new FileLogger(_path);

        logger.Info("created Ada");            // default-interface shorthand → Log(Info, ...)
        logger.Error("something broke");

        var text = File.ReadAllText(_path);
        Assert.Contains("[Info] created Ada", text);
        Assert.Contains("[Error] something broke", text);
    }

    [Fact]
    public void Log_AppendsRatherThanOverwrites()
    {
        // Typed as IAppLogger: the Info/Error shorthands are DEFAULT INTERFACE METHODS,
        // reachable only through the interface, not through the concrete FileLogger type.
        IAppLogger logger = new FileLogger(_path);

        logger.Info("first");
        logger.Info("second");

        var lines = File.ReadAllLines(_path);
        Assert.Equal(2, lines.Length);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
