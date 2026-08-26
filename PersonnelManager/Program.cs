using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;
using PersonnelManager.Presentation;

// ---------------------------------------------------------------------------
// COMPOSITION ROOT
// The one place that knows about concrete types. Every layer above depends only
// on abstractions; here we pick the implementations and wire the graph together.
// Swap InMemoryPersonalRepository for an EF Core one and nothing else changes.
// ---------------------------------------------------------------------------

// Infrastructure: a single shared store + a file logger, both live for the whole run.
var repository = new InMemoryPersonalRepository();
IAppLogger logger = new FileLogger(Path.Combine(AppContext.BaseDirectory, "personnel.log"));
logger.Info("Application started.");

// Application: build the service, then WRAP it in its logging decorator. Callers only ever see
// IPersonnelService, so they get logging for free without knowing a decorator is involved.
IPersonalValidator validator = new PersonalValidator();
IPersonnelService service = new LoggingPersonnelServiceDecorator(
    new PersonnelService(repository, validator), logger);

// Seed a couple of records so "List all" shows something on first run (these get logged too).
await service.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", "London", "+44 100"));
await service.CreateAsync(
    new CreatePersonalRequest("Alan", "Turing", "Manchester", "+44 200", EmploymentStatus.OnLeave));

// A durable backup that reads/writes a JSON file next to the executable, wrapped in logging.
var backupPath = Path.Combine(AppContext.BaseDirectory, "personnel.json");
IPersonnelBackup backup = new LoggingBackupDecorator(
    new JsonPersonnelBackup(repository, backupPath), logger);

// Presentation: inject the service + backup abstractions, then run the menu.
var app = new ConsoleApp(service, backup);
await app.RunAsync();

logger.Info("Application stopped.");
