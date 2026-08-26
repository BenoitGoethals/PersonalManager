using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Infrastructure;
using PersonnelManager.Presentation;

namespace PersonnelManager.Composition;

/// <summary>
/// Registers every service in the DI container. This replaces the hand-wired composition root:
/// instead of `new`-ing the whole object graph by hand, we declare each abstraction's
/// implementation and let the container resolve constructor dependencies for us.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersonnelManager(this IServiceCollection services, string dataDirectory)
    {
        // Infrastructure — the in-memory store is a singleton because it IS the data for the run.
        services.AddSingleton<IPersonalRepository, InMemoryPersonalRepository>();
        services.AddSingleton<IAppLogger>(_ => new FileLogger(Path.Combine(dataDirectory, "personnel.log")));

        // Application
        services.AddSingleton<IPersonalValidator, PersonalValidator>();

        // The service, exposed through its logging decorator. We register the concrete
        // PersonnelService, then register IPersonnelService as a factory that wraps it —
        // this is how you add a decorator with the built-in container.
        services.AddSingleton<PersonnelService>();
        services.AddSingleton<IPersonnelService>(sp =>
            new LoggingPersonnelServiceDecorator(
                sp.GetRequiredService<PersonnelService>(),
                sp.GetRequiredService<IAppLogger>()));

        // The backup, likewise wrapped in its logging decorator.
        services.AddSingleton<IPersonnelBackup>(sp =>
            new LoggingBackupDecorator(
                new JsonPersonnelBackup(
                    sp.GetRequiredService<IPersonalRepository>(),
                    Path.Combine(dataDirectory, "personnel.json")),
                sp.GetRequiredService<IAppLogger>()));

        // Presentation — its (IPersonnelService, IPersonnelBackup) constructor is satisfied above.
        services.AddSingleton<ConsoleApp>();

        return services;
    }
}
