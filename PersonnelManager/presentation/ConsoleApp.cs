using PersonnelManager.Application.Abstractions;
using PersonnelManager.Domain;

namespace PersonnelManager.Presentation;

/// <summary>
/// The user-facing layer. It knows nothing about how data is stored or how use cases work
/// internally — it only talks to the IPersonnelService and IPersonnelBackup abstractions injected
/// into its constructor. This is the Dependency Inversion Principle: the outermost layer depends
/// on abstractions defined further in.
/// </summary>
public sealed class ConsoleApp(IPersonnelService service, IPersonnelBackup backup)
{
    public async Task RunAsync()
    {
        Console.WriteLine("=== Personnel Manager (in-memory CRUD, C# 14) ===");

        while (true)
        {
            Console.WriteLine(
                """

                1) List all
                2) Get by id
                3) Create
                4) Update
                5) Delete
                6) Save to file
                7) Load from file
                0) Quit
                """);
            Console.Write("Choose > ");

            var choice = Console.ReadLine();
            if (choice is null)
            {
                // End of input stream (e.g. piped input exhausted or Ctrl+D) — quit cleanly
                // instead of looping forever on a null read.
                Console.WriteLine("Bye!");
                return;
            }

            var input = choice.Trim();

            // Switch expression over the raw input — constant patterns for the numbered menu.
            var quit = input switch
            {
                "1" => await ListAllAsync(),
                "2" => await GetByIdAsync(),
                "3" => await CreateAsync(),
                "4" => await UpdateAsync(),
                "5" => await DeleteAsync(),
                "6" => await SaveToFileAsync(),
                "7" => await LoadFromFileAsync(),
                "0" => true,
                // Anything else: try to read it as a typed command (see list patterns below).
                _ => await TryRunCommandAsync(input),
            };

            if (quit)
            {
                Console.WriteLine("Bye!");
                return;
            }
        }
    }

    private async Task<bool> ListAllAsync()
    {
        var people = await service.GetAllAsync();
        if (people.Count == 0)
        {
            Console.WriteLine("(no records yet)");
            return false;
        }

        foreach (var dto in people)
            Console.WriteLine(dto.ToDisplayLine());

        return false;
    }

    private async Task<bool> GetByIdAsync()
    {
        if (!TryReadId(out var id))
            return false;

        var result = await service.GetByIdAsync(id);
        // Result.Match forces us to handle both branches — no forgotten error case.
        Console.WriteLine(result.Match(
            onSuccess: dto => dto.ToDisplayLine(),
            onFailure: error => $"Error: {error}"));
        return false;
    }

    private async Task<bool> CreateAsync()
    {
        var request = new CreatePersonalRequest(
            Prompt("Name"), Prompt("Surname"), Prompt("Address"), Prompt("Phone"), PromptStatus());

        var result = await service.CreateAsync(request);
        Console.WriteLine(result.Match(
            onSuccess: dto => $"Created: {dto.ToDisplayLine()}",
            onFailure: error => $"Error: {error}"));
        return false;
    }

    private async Task<bool> UpdateAsync()
    {
        if (!TryReadId(out var id))
            return false;

        var request = new UpdatePersonalRequest(
            id, Prompt("Name"), Prompt("Surname"), Prompt("Address"), Prompt("Phone"), PromptStatus());

        var result = await service.UpdateAsync(request);
        Console.WriteLine(result.Match(
            onSuccess: dto => $"Updated: {dto.ToDisplayLine()}",
            onFailure: error => $"Error: {error}"));
        return false;
    }

    private async Task<bool> DeleteAsync()
    {
        if (!TryReadId(out var id))
            return false;

        var result = await service.DeleteAsync(id);
        Console.WriteLine(result.Match(
            onSuccess: deletedId => $"Deleted {deletedId}.",
            onFailure: error => $"Error: {error}"));
        return false;
    }

    private async Task<bool> SaveToFileAsync()
    {
        // 'await' here unwraps the Task the backup returns. The method pauses until the
        // file write completes, WITHOUT blocking the thread, then continues on the next line.
        await backup.SaveAsync();
        Console.WriteLine("Saved to file.");
        return false;
    }

    private async Task<bool> LoadFromFileAsync()
    {
        var count = await backup.RestoreAsync();
        Console.WriteLine($"Loaded {count} record(s) from file.");
        return false;
    }

    // Power-user command mode. Split the input into tokens, then match the LIST PATTERN —
    // the shape of the token array itself — to decide what the user meant.
    private async Task<bool> TryRunCommandAsync(string raw)
    {
        var tokens = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return tokens switch
        {
            // ["find", .. var terms]  → "find" then a SLICE capturing the remaining tokens.
            // The `when` guard rejects a bare "find" with no search term.
            ["find", .. var terms] when terms.Length > 0 => await FindAsync(terms),

            // ["delete", var id]      → exactly two tokens; the second is captured as `id`.
            ["delete", var id] => await DeleteByTextAsync(id),

            // ["help"]                → exactly one token equal to "help".
            ["help"] => ShowCommandHelp(),

            // []                      → empty input; and _ → anything else.
            [] => false,
            _ => Warn("Unknown choice."),
        };
    }

    private async Task<bool> FindAsync(string[] terms)
    {
        var needle = string.Join(' ', terms);
        var everyone = await service.GetAllAsync();

        var matches = everyone
            .Where(dto =>
                (dto.Name?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (dto.Surname?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        if (matches.Count == 0)
            Console.WriteLine("(no matches)");
        else
            foreach (var dto in matches)
                Console.WriteLine(dto.ToDisplayLine());

        return false;
    }

    private async Task<bool> DeleteByTextAsync(string idText)
    {
        if (!Guid.TryParse(idText, out var id))
            return Warn("That is not a valid Guid.");

        var result = await service.DeleteAsync(id);
        Console.WriteLine(result.Match(
            onSuccess: deletedId => $"Deleted {deletedId}.",
            onFailure: error => $"Error: {error}"));
        return false;
    }

    private static bool ShowCommandHelp()
    {
        Console.WriteLine("Commands:  find <term>  |  delete <id>  |  help");
        return false;
    }

    // --- small console helpers ---
    // (Formatting now lives in PersonnelDisplayExtensions: dto.ToDisplayLine(), status.Label.)

    private static string? Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine();
    }

    // Enum.TryParse turns text into the enum; blank/garbage falls back to the default.
    private static EmploymentStatus PromptStatus()
    {
        // The hint is built from the STATIC extension member EmploymentStatus.All — add a status
        // to the enum and this prompt updates itself.
        var options = string.Join("/", EmploymentStatus.All);
        Console.Write($"Status ({options}) [Active]: ");
        return Enum.TryParse<EmploymentStatus>(Console.ReadLine(), ignoreCase: true, out var status)
            ? status
            : EmploymentStatus.Active;
    }

    private static bool TryReadId(out Guid id)
    {
        Console.Write("Id (Guid): ");
        if (Guid.TryParse(Console.ReadLine(), out id))
            return true;

        Warn("That is not a valid Guid.");
        return false;
    }

    private static bool Warn(string message)
    {
        Console.WriteLine(message);
        return false;
    }
}
