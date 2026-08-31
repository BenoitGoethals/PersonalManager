using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PersonnelManager.Web.ApiClient;

namespace PersonnelManager.Web.Tests;

/// <summary>
/// Hosts the Web MVC app in-process and points its typed API <see cref="HttpClient"/> at a real,
/// in-memory instance of PersonnelManager.Api — so tests exercise the whole stack (cookie auth,
/// controllers, the HTTP client, JWT propagation, and the API's own validation/authorization)
/// without any sockets or database.
///
/// Both entry points are referenced by a public marker type rather than the (ambiguous, since both
/// assemblies define a global <c>Program</c>) entry class.
/// </summary>
public sealed class WebApiFactory : WebApplicationFactory<PersonnelApiClient>
{
    // Config read at builder-configure time (before ConfigureAppConfiguration) must arrive via
    // environment variables. Set once for the whole test process.
    static WebApiFactory()
    {
        Environment.SetEnvironmentVariable("Api__BaseUrl", "http://api.local");           // Web -> API base
        Environment.SetEnvironmentVariable("ConnectionStrings__Personnel", "");            // API -> in-memory store
        var dataDirectory = Path.Combine(Path.GetTempPath(), "pm-web-tests");
        Directory.CreateDirectory(dataDirectory);
        Environment.SetEnvironmentVariable("DataDirectory", dataDirectory);                // API -> file logger/backup
    }

    // Each factory instance owns its own API server (and thus a fresh in-memory store).
    private readonly ApiServer _api = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
            // Override only the primary handler of the "IPersonnelApiClient" typed client so its
            // requests are dispatched to the in-memory API server. The BearerTokenHandler (added in
            // the app's own registration) stays in the chain, so JWT propagation is still exercised.
            services.Configure<HttpClientFactoryOptions>(nameof(IPersonnelApiClient), options =>
                options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
                    handlerBuilder.PrimaryHandler = _api.Server.CreateHandler())));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _api.Dispose();
    }

    /// <summary>The in-memory PersonnelManager.Api server the Web app talks to.</summary>
    private sealed class ApiServer : WebApplicationFactory<PersonnelManager.Api.Auth.JwtOptions>;
}
