using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Composition;
using PersonnelManager.Domain;
using PersonnelManager.Presentation;

// ---------------------------------------------------------------------------
// COMPOSITION ROOT
// One ServiceCollection describes the whole object graph; the container resolves
// constructor dependencies for us. Swap any registration in ServiceCollectionExtensions
// and nothing here changes.
// ---------------------------------------------------------------------------

// The Postgres connection string comes from the environment — NEVER hard-code credentials.
//   export PERSONNEL_DB="Host=192.168.0.30;Database=personnel;Username=benoi;Password=..."
// If it's unset, the app falls back to the in-memory store.
var connectionString = Environment.GetEnvironmentVariable("PERSONNEL_DB");

var services = new ServiceCollection();
services.AddPersonnelManager(dataDirectory: AppContext.BaseDirectory, connectionString: connectionString);
services.AddSingleton<ConsoleApp>(); // presentation registration lives with the console host

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<IAppLogger>();
logger.Info($"Application started ({(string.IsNullOrWhiteSpace(connectionString) ? "in-memory" : "PostgreSQL")} store).");

// Seed a couple of records ONLY for the in-memory store (which starts empty every run).
// A real database persists across runs, so seeding it each launch would create duplicates.
if (string.IsNullOrWhiteSpace(connectionString))
{
    var service = provider.GetRequiredService<IPersonnelService>();
    await service.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", "London", "+44 100"));
    await service.CreateAsync(
        new CreatePersonalRequest("Alan", "Turing", "Manchester", "+44 200", EmploymentStatus.OnLeave));
}

// Resolve the fully-wired console app and run it.
var app = provider.GetRequiredService<ConsoleApp>();
await app.RunAsync();

logger.Info("Application stopped.");
