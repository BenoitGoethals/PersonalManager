using LogAnalyzer;
using Microsoft.Extensions.DependencyInjection;

namespace LogAnalyzer.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void Resolves_AnalyzerApp_AndItsServices()
    {
        using var provider = new ServiceCollection()
            .AddLogAnalyzer()
            .BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<AnalyzerApp>());
        Assert.IsType<LogFileAnalyzer>(provider.GetRequiredService<ILogFileAnalyzer>());
        Assert.IsType<ConsoleSummaryWriter>(provider.GetRequiredService<ISummaryWriter>());
    }
}
