using LogAnalyzer;
using Microsoft.Extensions.DependencyInjection;

const string defaultFolder = "sample-logs";

// Build the container and resolve the app; the command handlers get their collaborators injected.
await using var provider = new ServiceCollection()
    .AddLogAnalyzer()
    .BuildServiceProvider();

var app = provider.GetRequiredService<AnalyzerApp>();

// Dispatch on the shape of the argument list (list patterns).
return args switch
{
    ["generate", ..] => await app.GenerateAsync(Arg(args, 1) ?? defaultFolder, ArgInt(args, 2) ?? 10),
    ["analyze", var file] => await app.AnalyzeOneAsync(file),
    ["watch", ..] => await app.WatchAsync(Arg(args, 1) ?? defaultFolder),
    ["demo", ..] => await app.DemoAsync(Arg(args, 1) ?? defaultFolder),
    _ => Usage(),
};

static string? Arg(string[] a, int i) => i < a.Length ? a[i] : null;
static int? ArgInt(string[] a, int i) => i < a.Length && int.TryParse(a[i], out var n) ? n : null;

static int Usage()
{
    Console.WriteLine(
        """
        LogAnalyzer — watch and summarize log files.

        Usage:
          generate [folder] [count]   Write test .log files (default: sample-logs, 10)
          watch    [folder]           Analyze existing logs, then watch for new/changed files
          analyze  <file>             Summarize a single log file
          demo     [folder]           Generate 10 files, then watch while injecting changes
        """);
    return 0;
}
