using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Composition;
using PersonnelManager.Infrastructure;
using PersonnelManager.Infrastructure.Persistence;
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
        // The console host registers its own presentation type on top of the core registrations.
        using var provider = new ServiceCollection()
            .AddPersonnelManager(Path.GetTempPath())
            .AddSingleton<ConsoleApp>()
            .BuildServiceProvider();

        // If any core dependency were missing/mis-registered, resolving ConsoleApp would throw.
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

    [Fact]
    public void WithoutConnectionString_UsesInMemoryStore()
    {
        using var provider = BuildProvider();

        Assert.IsType<InMemoryPersonalRepository>(provider.GetRequiredService<IPersonalRepository>());
    }

    [Fact]
    public void WithConnectionString_UsesEfPostgresStore()
    {
        // A placeholder connection string is enough: resolving the repository does not open a
        // connection (Npgsql connects lazily on the first query).
        using var provider = new ServiceCollection()
            .AddPersonnelManager(Path.GetTempPath(), "Host=localhost;Database=personnel;Username=postgres")
            .BuildServiceProvider();

        Assert.IsType<EfPersonalRepository>(provider.GetRequiredService<IPersonalRepository>());
    }
}
