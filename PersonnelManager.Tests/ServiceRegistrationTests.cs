using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Composition;
using PersonnelManager.Presentation;

namespace PersonnelManager.Tests;

public class ServiceRegistrationTests
{
    private static ServiceProvider BuildProvider() =>
        new ServiceCollection()
            .AddPersonnelManager(Path.GetTempPath())
            .BuildServiceProvider();

    [Fact]
    public void Resolves_ConsoleApp_WithAllDependenciesSatisfied()
    {
        using var provider = BuildProvider();

        // If any dependency were missing/mis-registered, resolving ConsoleApp would throw.
        Assert.NotNull(provider.GetRequiredService<ConsoleApp>());
    }

    [Fact]
    public void PersonnelService_ResolvesToItsLoggingDecorator()
    {
        using var provider = BuildProvider();

        var service = provider.GetRequiredService<IPersonnelService>();

        Assert.IsType<LoggingPersonnelServiceDecorator>(service);
    }

    [Fact]
    public void Backup_ResolvesToItsLoggingDecorator()
    {
        using var provider = BuildProvider();

        Assert.IsType<LoggingBackupDecorator>(provider.GetRequiredService<IPersonnelBackup>());
    }

    [Fact]
    public void Repository_IsSingleton_SharedAcrossResolutions()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<IPersonalRepository>();
        var second = provider.GetRequiredService<IPersonalRepository>();

        Assert.Same(first, second); // one shared in-memory store for the whole run
    }
}
