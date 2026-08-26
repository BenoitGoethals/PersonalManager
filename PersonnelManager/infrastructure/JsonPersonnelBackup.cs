using System.Text.Json;
using System.Text.Json.Serialization;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure;

/// <summary>
/// Saves/loads the repository contents as a JSON file. Unlike the in-memory store (whose
/// "async" methods finish instantly via Task.FromResult), these methods do genuine disk I/O:
/// the awaits below can actually suspend this method and free the thread until the OS is done.
/// </summary>
public sealed class JsonPersonnelBackup(IPersonalRepository repository, string filePath)
    : IPersonnelBackup
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }, // write EmploymentStatus as "Active", not 0
    };

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // await #1 — completes synchronously (in-memory repo), but we still await the Task.
        var people = await repository.GetAllAsync(cancellationToken);

        // 'await using' asynchronously disposes the stream (flushes to disk) when the block ends.
        await using var stream = File.Create(filePath);

        // await #2 — REAL asynchronous I/O. While the OS writes bytes, this method yields the
        // thread instead of blocking it. Execution resumes here once the write finishes.
        await JsonSerializer.SerializeAsync(stream, people, Options, cancellationToken);
    }

    public async Task<int> RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return 0; // an async method can return a plain value; it's wrapped in a Task for you.

        await using var stream = File.OpenRead(filePath);
        var people = await JsonSerializer
            .DeserializeAsync<List<Personal>>(stream, Options, cancellationToken) ?? [];

        // Awaiting inside a loop: each Add is awaited before the next begins (sequential).
        foreach (var person in people)
            await repository.AddAsync(person, cancellationToken);

        return people.Count;
    }
}
