using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PersonnelManager.Application.Abstractions;
using PersonnelManager.Composition;
using PersonnelManager.Desktop.ViewModels;
using PersonnelManager.Desktop.Views;
using PersonnelManager.Domain;

namespace PersonnelManager.Desktop;

// Fully-qualified: the core's `PersonnelManager.Application` namespace would otherwise shadow
// the `Avalonia.Application` type here.
public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // Composition root for the desktop host: the SAME core wiring the console uses
        // (AddPersonnelManager), plus this UI's own presentation type (MainViewModel).
        var connectionString = Environment.GetEnvironmentVariable("PERSONNEL_DB");

        var services = new ServiceCollection();
        services.AddPersonnelManager(AppContext.BaseDirectory, connectionString);
        services.AddSingleton<MainViewModel>();
        var provider = services.BuildServiceProvider();

        // Seed the in-memory store so the window shows data on first launch (skipped for a real DB).
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var service = provider.GetRequiredService<IPersonnelService>();
            service.CreateAsync(new CreatePersonalRequest("Ada", "Lovelace", "London", "+44 100"))
                .GetAwaiter().GetResult();
            service.CreateAsync(new CreatePersonalRequest("Alan", "Turing", "Manchester", "+44 200", EmploymentStatus.OnLeave))
                .GetAwaiter().GetResult();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
