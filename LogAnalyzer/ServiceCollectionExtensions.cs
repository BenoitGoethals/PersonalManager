using Microsoft.Extensions.DependencyInjection;

namespace LogAnalyzer;

/// <summary>Registers the LogAnalyzer services in a DI container.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogAnalyzer(this IServiceCollection services)
    {
        services.AddSingleton<ILogFileAnalyzer, LogFileAnalyzer>();
        services.AddSingleton<ISampleLogGenerator, SampleLogGenerator>();
        services.AddSingleton<ISummaryWriter, ConsoleSummaryWriter>();
        services.AddSingleton<AnalyzerApp>();
        return services;
    }
}
