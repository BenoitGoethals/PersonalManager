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

var services = new ServiceCollection();
services.AddPersonnelManager(dataDirectory: AppContext.BaseDirectory);

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<IAppLogger>();
logger.Info("Application started.");

// Seed a couple of records through the service so "List all" shows something on first run.
var service = provider.GetRequiredService<IPersonnelService>();
await service.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", "London", "+44 100"));
await service.CreateAsync(
    new CreatePersonalRequest("Alan", "Turing", "Manchester", "+44 200", EmploymentStatus.OnLeave));

// Resolve the fully-wired console app and run it.
var app = provider.GetRequiredService<ConsoleApp>();
await app.RunAsync();

logger.Info("Application stopped.");
