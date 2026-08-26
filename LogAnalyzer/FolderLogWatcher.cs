using System.Collections.Concurrent;

namespace LogAnalyzer;

/// <summary>
/// Watches a folder for new or changed *.log files and invokes a callback for each.
/// FileSystemWatcher raises several events per write, so changes are debounced per file.
/// </summary>
public sealed class FolderLogWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly Func<string, Task> _onChanged;
    private readonly ConcurrentDictionary<string, DateTime> _lastHandled = new();
    private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(400);

    public FolderLogWatcher(string folder, Func<string, Task> onChanged)
    {
        _onChanged = onChanged;
        Directory.CreateDirectory(folder);

        _watcher = new FileSystemWatcher(folder, "*.log")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
        };
        _watcher.Created += OnEvent;
        _watcher.Changed += OnEvent;
        _watcher.Renamed += OnEvent;
    }

    public void Start() => _watcher.EnableRaisingEvents = true;

    private void OnEvent(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.Now;
        var last = _lastHandled.GetValueOrDefault(e.FullPath);
        if (now - last < _debounce)
            return; // duplicate event within the debounce window — ignore
        _lastHandled[e.FullPath] = now;

        // The FileSystemWatcher event is synchronous; run the async analysis without blocking it,
        // and never let an exception escape onto the watcher's thread.
        _ = HandleAsync(e.FullPath);
    }

    private async Task HandleAsync(string path)
    {
        try
        {
            await _onChanged(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ! could not analyze {Path.GetFileName(path)}: {ex.Message}");
        }
    }

    public void Dispose() => _watcher.Dispose();
}
