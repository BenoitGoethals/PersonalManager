using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonnelManager.Infrastructure.Persistence;

namespace PersonnelManager.Api.Health;

/// <summary>
/// Reports whether the personnel data store is reachable. When PostgreSQL is configured, it opens a
/// short-lived context via the registered <see cref="IDbContextFactory{TContext}"/> and pings the
/// database. When the in-memory store is used (no connection string), there is nothing to reach, so
/// it reports healthy with a note.
/// </summary>
public sealed class DataStoreHealthCheck(IServiceProvider services) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var factory = services.GetService<IDbContextFactory<PersonnelDbContext>>();
        if (factory is null)
            return HealthCheckResult.Healthy("Using in-memory store.");

        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("PostgreSQL reachable.")
                : HealthCheckResult.Unhealthy("PostgreSQL not reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL check failed.", ex);
        }
    }
}
