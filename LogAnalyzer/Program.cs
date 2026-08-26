using LogAnalyzer;

const string defaultFolder = "sample-logs";

// Dispatch on the shape of the argument list (list patterns).
var exit = args switch
{
    ["generate", ..] => await GenerateAsync(Arg(args, 1) ?? defaultFolder, ArgInt(args, 2) ?? 10),
    ["analyze", var file] => await AnalyzeOneAsync(file),
    ["watch", ..] => await WatchAsync(Arg(args, 1) ?? defaultFolder),
    ["demo", ..] => await DemoAsync(Arg(args, 1) ?? defaultFolder),
    _ => Usage(),
};
return exit;

async Task<int> GenerateAsync(string folder, int count)
{
    var files = await SampleLogGenerator.GenerateAsync(folder, count);
    Console.WriteLine($"Generated {files.Count} log file(s) in {Path.GetFullPath(folder)}:");
    foreach (var path in files)
        Console.WriteLine($"  {Path.GetFileName(path)}");
    return 0;
}

async Task<int> AnalyzeOneAsync(string file)
{
    if (!File.Exists(file))
    {
        Console.WriteLine($"No such file: {file}");
        return 1;
    }

    SummaryPrinter.Print(await LogFileAnalyzer.AnalyzeAsync(file));
    return 0;
}

async Task<int> WatchAsync(string folder)
{
    var full = Path.GetFullPath(folder);
    Directory.CreateDirectory(full);

    Console.WriteLine($"Scanning existing logs in {full} ...");
    foreach (var path in Directory.EnumerateFiles(full, "*.log").Order())
        SummaryPrinter.Print(await LogFileAnalyzer.AnalyzeAsync(path));

    using var watcher = new FolderLogWatcher(full, AnalyzeOnChangeAsync);
    watcher.Start();

    Console.WriteLine("\nWatching for new / changed *.log files. Press Enter to stop.");
    Console.ReadLine();
    return 0;
}

// A self-contained demo: generate → watch → inject a change and a new file so you can see it react.
async Task<int> DemoAsync(string folder)
{
    var full = Path.GetFullPath(folder);
    await SampleLogGenerator.GenerateAsync(full, 10);
    Console.WriteLine($"Generated 10 files in {full}. Watching...");

    using var watcher = new FolderLogWatcher(full, AnalyzeOnChangeAsync);
    watcher.Start();

    await Task.Delay(700);
    var existing = Directory.EnumerateFiles(full, "*.log").Order().First();
    await File.AppendAllTextAsync(existing,
        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Error] [demo] injected failure{Environment.NewLine}");

    await Task.Delay(700);
    await File.WriteAllTextAsync(Path.Combine(full, "live-99.log"),
        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Fatal] [demo] a brand new file appeared{Environment.NewLine}");

    await Task.Delay(1000);
    Console.WriteLine("\nDemo complete.");
    return 0;
}

async Task AnalyzeOnChangeAsync(string path)
{
    Console.WriteLine($"\n[change detected] {Path.GetFileName(path)}");
    SummaryPrinter.Print(await LogFileAnalyzer.AnalyzeAsync(path));
}

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
