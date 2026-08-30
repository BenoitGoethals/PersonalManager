using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PersonnelManager.Api.Contracts;

namespace PersonnelManager.Api.Tests;

/// <summary>
/// Boots the real API in-process for integration tests, backed by the in-memory store.
///
/// Program.cs reads the connection string and data directory at builder-configure time — which
/// runs before WebApplicationFactory's ConfigureAppConfiguration callbacks — so those overrides
/// are supplied as environment variables (a configuration source that IS read that early) rather
/// than via ConfigureAppConfiguration. JWT/user config comes from the API's own appsettings.json.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    static ApiFactory()
    {
        // Blank connection string => in-memory store (no PostgreSQL needed, health reports healthy).
        Environment.SetEnvironmentVariable("ConnectionStrings__Personnel", "");
        // Keep the file logger / JSON backup out of the source tree.
        var dataDirectory = Path.Combine(Path.GetTempPath(), "pm-api-tests");
        Directory.CreateDirectory(dataDirectory);
        Environment.SetEnvironmentVariable("DataDirectory", dataDirectory);
    }

    /// <summary>JSON options mirroring the API (string enums, case-insensitive) for test (de)serialization.</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");

    /// <summary>Create an <see cref="HttpClient"/> already carrying a bearer token for the given user.</summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }
}
