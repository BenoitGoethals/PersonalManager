using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Infrastructure;
using PersonnelManager.Tests.Fakes;

namespace PersonnelManager.Tests;

public class LoggingPersonnelServiceDecoratorTests
{
    [Fact]
    public async Task LogsBeforeAndAfter_ASuccessfulCall()
    {
        var logger = new RecordingLogger();
        var inner = new PersonnelService(new InMemoryPersonalRepository(), new PersonalValidator());
        var sut = new LoggingPersonnelServiceDecorator(inner, logger);

        var result = await sut.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", null, null));

        Assert.True(result.IsSuccess); // the decorator is transparent — same result as the inner service
        Assert.Contains(logger.Entries, e => e.Message.Contains("CreateAsync starting"));
        Assert.Contains(logger.Entries, e => e.Message.Contains("CreateAsync completed"));
        Assert.All(logger.Entries, e => Assert.Equal(LogLevel.Info, e.Level));
    }

    [Fact]
    public async Task LogsError_ThenRethrows()
    {
        var logger = new RecordingLogger();
        var sut = new LoggingPersonnelServiceDecorator(new ThrowingPersonnelService(), logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.DeleteAsync(Guid.NewGuid()));

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("boom"));
    }
}
