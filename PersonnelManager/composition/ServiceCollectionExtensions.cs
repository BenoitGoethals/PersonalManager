using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;
using PersonnelManager.Infrastructure.Persistence;

namespace PersonnelManager.Composition;

/// <summary>
/// Registers the CORE services (repository, logger, validator, service, backup) in the DI container.
/// This lives in the Core library so every front-end (Console, Avalonia, …) shares one wiring.
/// Each UI host then registers its OWN presentation types on top of this.
/// </summary>
public static class ServiceCollectionExtensions
{
    // When 'connectionString' is supplied, personnel are stored in PostgreSQL via EF Core; otherwise
    // an in-memory store is used. This is the whole point of the IPersonalRepository abstraction —
    // swapping the store is one branch here, and nothing above infrastructure changes.
    public static IServiceCollection AddPersonnelManager(
        this IServiceCollection services, string dataDirectory, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // In-memory store — a singleton because it IS the data for the run.
            services.AddSingleton<IPersonalRepository, InMemoryPersonalRepository>();
        }
        else
        {
            // PostgreSQL via EF Core. The factory hands out a short-lived DbContext per operation.
            services.AddDbContextFactory<PersonnelDbContext>(options => options.UseNpgsql(connectionString));
            services.AddSingleton<IPersonalRepository, EfPersonalRepository>();
        }

        services.AddSingleton<IAppLogger>(_ => new FileLogger(Path.Combine(dataDirectory, "personnel.log")));

        // Application — the FluentValidation rule set. Validators are stateless and thread-safe,
        // so a singleton is fine and lets the container inject IValidator<Personal> anywhere.
        services.AddSingleton<IValidator<Personal>, PersonalValidator>();

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

        // Presentation types are registered by each UI host, not here — that keeps the Core
        // library free of any dependency on a specific front-end.
        return services;
    }
}
