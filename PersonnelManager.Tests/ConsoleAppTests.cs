using PersonnelManager.Application.Abstractions;
using PersonnelManager.Application.Personnel;
using PersonnelManager.Domain;
using PersonnelManager.Infrastructure;
using PersonnelManager.Presentation;
using PersonnelManager.Tests.Fakes;

namespace PersonnelManager.Tests;

/// <summary>
/// Tests for the interactive menu. ConsoleApp talks to the static Console, so each test
/// redirects Console.In/Out through a harness and restores them afterwards. Because that
/// static state is shared, this class is marked non-parallel.
///
/// Every input script MUST end with "0" (Quit) — or rely on the EOF-quit behaviour — so the
/// menu loop terminates.
/// </summary>
[Collection(nameof(ConsoleAppTests))]
[CollectionDefinition(nameof(ConsoleAppTests), DisableParallelization = true)]
public class ConsoleAppTests
{
    private static ConsoleApp BuildApp(IPersonalRepository repository, IPersonnelBackup? backup = null)
    {
        var service = new PersonnelService(repository, new PersonalValidator());
        return new ConsoleApp(service, backup ?? new RecordingBackup());
    }

    /// <summary>Runs the app against a scripted stdin and returns everything it wrote to stdout.</summary>
    private static async Task<string> RunAsync(ConsoleApp app, string input)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        try
        {
            using var reader = new StringReader(input);
            using var writer = new StringWriter();
            Console.SetIn(reader);
            Console.SetOut(writer);

            await app.RunAsync();
            return writer.ToString();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task QuitImmediately_PrintsGoodbye()
    {
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "0\n");

        Assert.Contains("Bye!", output);
    }

    [Fact]
    public async Task EndOfInput_QuitsWithoutLooping()
    {
        // No "0" — the input simply ends. The EOF guard must stop the loop.
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "");

        Assert.Contains("Bye!", output);
    }

    [Fact]
    public async Task UnknownChoice_IsReported()
    {
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "9\n0\n");

        Assert.Contains("Unknown choice.", output);
    }

    [Fact]
    public async Task ListAll_WhenEmpty_ShowsPlaceholder()
    {
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "1\n0\n");

        Assert.Contains("(no records yet)", output);
    }

    [Fact]
    public async Task Create_ThenList_ShowsThePerson()
    {
        var script = string.Join('\n',
            "3", "Grace", "Hopper", "New York", "+1 555", "Active", // create (now prompts for status)
            "1",                                                    // list
            "0");                                                   // quit

        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), script);

        Assert.Contains("Created:", output);
        Assert.Contains("Grace Hopper", output);
    }

    [Fact]
    public async Task Create_WithStatusInput_IsParsedAndShown()
    {
        var script = string.Join('\n',
            "3", "Nadia", "Boulanger", "Paris", "+33 1", "OnLeave", // create with a non-default status
            "0");                                                   // quit

        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), script);

        Assert.Contains("on leave", output); // the switch-expression label, not the raw enum name
    }

    [Fact]
    public async Task SaveOption_InvokesBackupSave()
    {
        var backup = new RecordingBackup();
        var app = BuildApp(new InMemoryPersonalRepository(), backup);

        var output = await RunAsync(app, "6\n0\n");

        Assert.Equal(1, backup.SaveCalls);
        Assert.Contains("Saved to file.", output);
    }

    [Fact]
    public async Task LoadOption_InvokesBackupRestore_AndReportsCount()
    {
        var backup = new RecordingBackup { RestoreReturns = 3 };
        var app = BuildApp(new InMemoryPersonalRepository(), backup);

        var output = await RunAsync(app, "7\n0\n");

        Assert.Equal(1, backup.RestoreCalls);
        Assert.Contains("Loaded 3 record(s) from file.", output);
    }

    [Fact]
    public async Task FindCommand_ListsOnlyMatches()
    {
        var repository = new InMemoryPersonalRepository();
        await repository.AddAsync(new Personal { Name = "Ada", Surname = "Lovelace" });
        await repository.AddAsync(new Personal { Name = "Alan", Surname = "Turing" });

        var output = await RunAsync(BuildApp(repository), "find turing\n0\n");

        Assert.Contains("Alan Turing", output);
        Assert.DoesNotContain("Ada Lovelace", output);
    }

    [Fact]
    public async Task FindCommand_NoMatch_ShowsPlaceholder()
    {
        var repository = new InMemoryPersonalRepository();
        await repository.AddAsync(new Personal { Name = "Ada", Surname = "Lovelace" });

        var output = await RunAsync(BuildApp(repository), "find zzzz\n0\n");

        Assert.Contains("(no matches)", output);
    }

    [Fact]
    public async Task FindCommand_WithoutTerm_FailsGuard_AndReportsUnknown()
    {
        // tokens = ["find"] — the `when terms.Length > 0` guard rejects it, so it falls to `_`.
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "find\n0\n");

        Assert.Contains("Unknown choice.", output);
    }

    [Fact]
    public async Task DeleteCommand_RemovesPerson()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Grace", Surname = "Hopper" };
        await repository.AddAsync(person);

        var output = await RunAsync(BuildApp(repository), $"delete {person.Id}\n0\n");

        Assert.Contains($"Deleted {person.Id}", output);
        Assert.Null(await repository.GetByIdAsync(person.Id));
    }

    [Fact]
    public async Task HelpCommand_ListsCommands()
    {
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "help\n0\n");

        Assert.Contains("find <term>", output);
    }

    [Fact]
    public async Task GetById_WithInvalidGuid_IsReported()
    {
        var output = await RunAsync(BuildApp(new InMemoryPersonalRepository()), "2\nnot-a-guid\n0\n");

        Assert.Contains("That is not a valid Guid.", output);
    }

    [Fact]
    public async Task GetById_ExistingPerson_ShowsRecord()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Ada", Surname = "Lovelace" };
        await repository.AddAsync(person);

        var script = string.Join('\n', "2", person.Id.ToString(), "0");
        var output = await RunAsync(BuildApp(repository), script);

        Assert.Contains("Ada Lovelace", output);
    }

    [Fact]
    public async Task Delete_ExistingPerson_ConfirmsAndRemoves()
    {
        var repository = new InMemoryPersonalRepository();
        var person = new Personal { Name = "Alan", Surname = "Turing" };
        await repository.AddAsync(person);

        var script = string.Join('\n', "5", person.Id.ToString(), "0");
        var output = await RunAsync(BuildApp(repository), script);

        Assert.Contains($"Deleted {person.Id}", output);
        Assert.Null(await repository.GetByIdAsync(person.Id));
    }
}
