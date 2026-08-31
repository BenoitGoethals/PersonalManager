using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonnelManager.Web.Tests;

/// <summary>
/// End-to-end tests through the Web MVC front-end, which in turn drives the in-memory API.
/// Each test uses its own <see cref="WebApiFactory"/> (and therefore a fresh API data store).
/// </summary>
public sealed class WebIntegrationTests
{
    private static readonly WebApplicationFactoryClientOptions NoRedirect = new() { AllowAutoRedirect = false };

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Key, f.Value)));

    /// <summary>Log in and return the cookie-carrying client for that user.</summary>
    private static async Task<HttpClient> LoginAsync(WebApiFactory factory, string username, string password)
    {
        var client = factory.CreateClient(NoRedirect);
        var response = await client.PostAsync("/Account/Login", Form(("Username", username), ("Password", password)));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Personnel/Index", response.Headers.Location?.OriginalString);
        return client;
    }

    private static async Task<Guid> CreatePersonAsync(
        HttpClient client, string name, string surname, string status = "Active")
    {
        var create = await client.PostAsync("/Personnel/Create",
            Form(("Name", name), ("Surname", surname), ("Phone", "555"), ("Status", status)));
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);

        var html = await client.GetStringAsync("/Personnel/Index");
        var row = Regex.Matches(html, "<tr>.*?</tr>", RegexOptions.Singleline)
            .First(m => m.Value.Contains(name) && m.Value.Contains(surname)).Value;
        return Guid.Parse(Regex.Match(row, @"/Personnel/Edit/([0-9a-fA-F-]{36})").Groups[1].Value);
    }

    [Fact]
    public async Task Login_Get_ReturnsLoginForm()
    {
        using var factory = new WebApiFactory();
        var client = factory.CreateClient();

        var html = await client.GetStringAsync("/Account/Login");

        Assert.Contains("Log in", html);
    }

    [Fact]
    public async Task Login_WithBadCredentials_ReturnsErrorOnPage()
    {
        using var factory = new WebApiFactory();
        var client = factory.CreateClient(NoRedirect);

        var response = await client.PostAsync("/Account/Login", Form(("Username", "admin"), ("Password", "nope")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // re-renders the view, no redirect
        Assert.Contains("Invalid username or password.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Personnel_WhenAnonymous_RedirectsToLogin()
    {
        using var factory = new WebApiFactory();
        var client = factory.CreateClient(NoRedirect);

        var response = await client.GetAsync("/Personnel/Index");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Home_RedirectsToPersonnel()
    {
        using var factory = new WebApiFactory();
        var client = factory.CreateClient(NoRedirect);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Personnel", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Admin_CanCreate_AndPersonAppearsInList()
    {
        using var factory = new WebApiFactory();
        var client = await LoginAsync(factory, "admin", "admin123");

        await CreatePersonAsync(client, "Ada", "Lovelace");

        var html = await client.GetStringAsync("/Personnel/Index");
        Assert.Contains("Ada", html);
        Assert.Contains("Lovelace", html);
    }

    [Fact]
    public async Task Create_WithNoNameOrSurname_ShowsApiValidationMessage()
    {
        using var factory = new WebApiFactory();
        var client = await LoginAsync(factory, "admin", "admin123");

        var response = await client.PostAsync("/Personnel/Create",
            Form(("Name", ""), ("Surname", ""), ("Phone", "1"), ("Status", "Active")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // re-renders with errors, no redirect
        Assert.Contains("at least a name or a surname", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Edit_ChangesNameAndStatus()
    {
        using var factory = new WebApiFactory();
        var client = await LoginAsync(factory, "admin", "admin123");
        var id = await CreatePersonAsync(client, "Ada", "Lovelace");

        var edit = await client.PostAsync($"/Personnel/Edit/{id}",
            Form(("Name", "Grace"), ("Surname", "Hopper"), ("Phone", "555"), ("Status", "OnLeave")));
        Assert.Equal(HttpStatusCode.Redirect, edit.StatusCode);

        var html = await client.GetStringAsync("/Personnel/Index");
        Assert.Contains("Grace", html);
        Assert.Contains("Hopper", html);
        Assert.Contains("OnLeave", html);
    }

    [Fact]
    public async Task User_DoesNotSeeDeleteOrBackup_AndIsBlockedFromDeletePage()
    {
        using var factory = new WebApiFactory();
        var admin = await LoginAsync(factory, "admin", "admin123");
        var id = await CreatePersonAsync(admin, "Ada", "Lovelace");

        var user = await LoginAsync(factory, "user", "user123");
        var listHtml = await user.GetStringAsync("/Personnel/Index");
        Assert.DoesNotContain(">Delete<", listHtml);
        Assert.DoesNotContain("Back up to JSON", listHtml);

        var deletePage = await user.GetAsync($"/Personnel/Delete/{id}");
        Assert.Equal(HttpStatusCode.Redirect, deletePage.StatusCode); // Admin-only -> access denied -> login
        Assert.Contains("/Account/Login", deletePage.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Admin_CanDelete_AndPersonIsRemoved()
    {
        using var factory = new WebApiFactory();
        var client = await LoginAsync(factory, "admin", "admin123");
        var id = await CreatePersonAsync(client, "Zara", "Unique");

        var delete = await client.PostAsync($"/Personnel/Delete/{id}", Form());
        Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);

        var html = await client.GetStringAsync("/Personnel/Index");
        Assert.DoesNotContain("Zara", html);
    }

    [Fact]
    public async Task Admin_CanTriggerBackup()
    {
        using var factory = new WebApiFactory();
        var client = await LoginAsync(factory, "admin", "admin123");
        await CreatePersonAsync(client, "Ada", "Lovelace");

        var backup = await client.PostAsync("/Personnel/Backup", Form());

        Assert.Equal(HttpStatusCode.Redirect, backup.StatusCode);
        Assert.StartsWith("/Personnel", backup.Headers.Location?.OriginalString);
    }
}
