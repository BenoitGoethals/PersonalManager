using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonnelManager.Infrastructure.Persistence;

/// <summary>
/// Used ONLY by the EF Core command-line tools (e.g. `dotnet ef migrations add`). It supplies a
/// context configured with a placeholder connection string so migrations can be generated without
/// a live database and without embedding real credentials — migrations don't connect to anything.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PersonnelDbContext>
{
    public PersonnelDbContext CreateDbContext(string[] args)
    {
        // `migrations add` doesn't connect, so the placeholder is fine offline. `database update`
        // DOES connect — set PERSONNEL_DB so the tool targets the real server (keeps the secret
        // out of source).
        var connectionString = Environment.GetEnvironmentVariable("PERSONNEL_DB")
            ?? "Host=localhost;Database=personnel;Username=postgres";

        var options = new DbContextOptionsBuilder<PersonnelDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PersonnelDbContext(options);
    }
}
