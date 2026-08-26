namespace LogAnalyzer;

/// <summary>
/// The application service that drives each CLI command. Its three collaborators are injected —
/// no `new`, no statics — so it's fully testable and its behaviour is composed by the container.
/// </summary>
public sealed class AnalyzerApp(
    ILogFileAnalyzer analyzer,
    ISampleLogGenerator generator,
    ISummaryWriter writer)
{
    public async Task<int> GenerateAsync(string folder, int count)
    {
        var files = await generator.GenerateAsync(folder, count);
        Console.WriteLine($"Generated {files.Count} log file(s) in {Path.GetFullPath(folder)}:");
        foreach (var path in files)
            Console.WriteLine($"  {Path.GetFileName(path)}");
        return 0;
    }

    public async Task<int> AnalyzeOneAsync(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine($"No such file: {file}");
            return 1;
        }

        writer.Write(await analyzer.AnalyzeAsync(file));
        return 0;
    }

    public async Task<int> WatchAsync(string folder)
    {
        var full = Path.GetFullPath(folder);
        Directory.CreateDirectory(full);

        Console.WriteLine($"Scanning existing logs in {full} ...");
        foreach (var path in Directory.EnumerateFiles(full, "*.log").Order())
            writer.Write(await analyzer.AnalyzeAsync(path));

        using var watcher = new FolderLogWatcher(full, AnalyzeAndWriteAsync);
        watcher.Start();

        Console.WriteLine("\nWatching for new / changed *.log files. Press Enter to stop.");
        Console.ReadLine();
        return 0;
    }

    // Self-contained demo: generate → watch → inject a change and a new file so you can see it react.
    public async Task<int> DemoAsync(string folder)
    {
        var full = Path.GetFullPath(folder);
        await generator.GenerateAsync(full, 10);
        Console.WriteLine($"Generated 10 files in {full}. Watching...");

        using var watcher = new FolderLogWatcher(full, AnalyzeAndWriteAsync);
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

    private async Task AnalyzeAndWriteAsync(string path)
    {
        Console.WriteLine($"\n[change detected] {Path.GetFileName(path)}");
        writer.Write(await analyzer.AnalyzeAsync(path));
    }
}
