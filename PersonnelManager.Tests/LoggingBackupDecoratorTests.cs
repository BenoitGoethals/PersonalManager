using PersonnelManager.Application.Abstractions;
using PersonnelManager.Tests.Fakes;

namespace PersonnelManager.Tests;

public class LoggingBackupDecoratorTests
{
    [Fact]
    public async Task Save_DelegatesToInner_AndLogs()
    {
        var logger = new RecordingLogger();
        var inner = new RecordingBackup();
        var sut = new LoggingBackupDecorator(inner, logger);

        await sut.SaveAsync();

        Assert.Equal(1, inner.SaveCalls); // transparent: the real save still happened
        Assert.Contains(logger.Entries, e => e.Message.Contains("Saving personnel"));
        Assert.Contains(logger.Entries, e => e.Message.Contains("Saved personnel"));
    }

    [Fact]
    public async Task Restore_PassesThroughCount_AndLogsIt()
    {
        var logger = new RecordingLogger();
        var inner = new RecordingBackup { RestoreReturns = 5 };
        var sut = new LoggingBackupDecorator(inner, logger);

        var count = await sut.RestoreAsync();

        Assert.Equal(5, count); // the decorator returns the inner value unchanged
        Assert.Contains(logger.Entries, e => e.Message.Contains("Restored 5 record(s)"));
    }

    [Fact]
    public async Task Save_WhenInnerThrows_LogsError_AndRethrows()
    {
        var logger = new RecordingLogger();
        var sut = new LoggingBackupDecorator(new ThrowingBackup(), logger);

        await Assert.ThrowsAsync<IOException>(() => sut.SaveAsync());

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("disk full"));
    }
}
