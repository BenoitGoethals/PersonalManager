using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;

namespace PersonnelManager.Tests;

/// <summary>
/// Exercises the genuinely-async file backup end-to-end against a real temp file.
/// Note the test methods are themselves `async Task` and `await` the operations — that's how
/// you test asynchronous code: await it, then assert. xUnit understands async test methods.
/// </summary>
public class JsonPersonnelBackupTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"personnel-{Guid.NewGuid():N}.json");

    [Fact]
    public async Task Save_ThenRestore_RoundTripsAllRecords()
    {
        var source = new InMemoryPersonalRepository();
        await source.AddAsync(new Personal { Name = "Ada", Surname = "Lovelace" });
        await source.AddAsync(new Personal
        {
            Name = "Alan", Surname = "Turing", Status = EmploymentStatus.OnLeave,
        });

        // Save from one repository...
        await new JsonPersonnelBackup(source, _path).SaveAsync();

        // ...restore into a brand-new, empty one.
        var target = new InMemoryPersonalRepository();
        var count = await new JsonPersonnelBackup(target, _path).RestoreAsync();

        Assert.Equal(2, count);
        var restored = await target.GetAllAsync();
        Assert.Equal(2, restored.Count);
        Assert.Contains(restored, p => p.Surname == "Turing" && p.Status == EmploymentStatus.OnLeave);
    }

    [Fact]
    public async Task Restore_WhenFileMissing_ReturnsZero()
    {
        var backup = new JsonPersonnelBackup(new InMemoryPersonalRepository(), _path);

        var count = await backup.RestoreAsync();

        Assert.Equal(0, count);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
